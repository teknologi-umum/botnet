using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Github;
using BotNet.Services.Github.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BotNet.Services.SafeSearch {
	public class SafeSearchDictionary {
		private static readonly char[] CONTENT_DELIMITERS = { ' ', '\t', '\r', '\n', '.', ',', ':', ';', '"', '@', '(', ')', '|', '-', '_', '/', '?', '!', '&', '#', '+' };
		private static readonly object DISALLOWED = new();
		private readonly IServiceProvider _serviceProvider;
		private readonly SemaphoreSlim _semaphore = new(1, 1);
		private ImmutableHashSet<string>? _disallowedWebsites;
		private ImmutableHashSet<string>? _disallowedWords;
		private ImmutableDictionary<string, ImmutableHashSet<string>>? _disallowedPhrases;

		public SafeSearchDictionary(
			IServiceProvider serviceProvider
		) {
			_serviceProvider = serviceProvider;
		}

		public async Task<bool> IsUrlAllowedAsync(string url, CancellationToken cancellationToken) {
			await EnsureInitializedAsync(cancellationToken);

			// strip protocol
			if (url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)) url = url[7..];
			if (url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)) url = url[8..];

			// extract domain name
			int slashPos = url.IndexOf('/', StringComparison.InvariantCultureIgnoreCase);
			if (slashPos != -1) url = url[..slashPos];

			string[] domainParts = url.Split('.');
			int subdomainsSkipped = 0;

			do {
				string domain = string.Join('.', domainParts.Skip(subdomainsSkipped));
				if (_disallowedWebsites!.Contains(domain)) return false;
				subdomainsSkipped++;
			} while (subdomainsSkipped < domainParts.Length - 1);

			return true;
		}

		public async Task<bool> IsContentAllowedAsync(string content, CancellationToken cancellationToken) {
			await EnsureInitializedAsync(cancellationToken);
			string[] words = content.Split(CONTENT_DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
			return !words.Any(word => {
				if (_disallowedWords!.Contains(word)) return true;
				if (_disallowedWords.Where(disallowedWord => disallowedWord.Length >= 4).Any(disallowedWord => word.StartsWith(disallowedWord, StringComparison.InvariantCultureIgnoreCase))) return true;
				if (_disallowedPhrases!.TryGetValue(word, out ImmutableHashSet<string>? phrases)) {
					foreach (string phrase in phrases) {
						string[] phraseWords = phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);
						if (phraseWords.All(phraseWord => words.Contains(phraseWord, StringComparer.InvariantCultureIgnoreCase))) return true;
					}
				}
				return false;
			});
		}

		public async Task EnsureInitializedAsync(CancellationToken cancellationToken) {
			await _semaphore.WaitAsync(cancellationToken);
			try {
				if (_disallowedWebsites == null
					|| _disallowedWords == null
					|| _disallowedPhrases == null) {
					GithubClient githubClient = _serviceProvider.GetRequiredService<GithubClient>();
					HttpClient httpClient = _serviceProvider.GetRequiredService<HttpClient>();
					SafeSearchOptions safeSearchOptions = _serviceProvider.GetRequiredService<IOptions<SafeSearchOptions>>().Value;

					if (safeSearchOptions.DisallowedWordsPath is null) {
						throw new InvalidOperationException("Disallowed words path not configured. Please add a .NET secret with key 'SafeSearchOptions:DisallowedWordsPath' or a Docker secret with key 'SafeSearchOptions__DisallowedWordsPath'");
					}

					// Disallowed websites

					ImmutableList<GithubContent> disallowedWebsitesFiles = await githubClient.GetContentAsync(
						owner: safeSearchOptions.BadWordListOwner ?? throw new InvalidOperationException("Bad word list owner name not configured. Please add a .NET secret with key 'SafeSearchOptions:BadWordListOwner' or a Docker secret with key 'SafeSearchOptions__BadWordListOwner'"),
						repo: safeSearchOptions.BadWordListRepository ?? throw new InvalidOperationException("Bad word list repository name not configured. Please add a .NET secret with key 'SafeSearchOptions:BadWordListRepository' or a Docker secret with key 'SafeSearchOptions__BadWordListRepository'"),
						path: safeSearchOptions.DisallowedWebsitesPath ?? throw new InvalidOperationException("Disallowed websites path not configured. Please add a .NET secret with key 'SafeSearchOptions:DisallowedWebsitesPath' or a Docker secret with key 'SafeSearchOptions__DisallowedWebsitesPath'"),
						cancellationToken: cancellationToken);

					string[] disallowedWebsitesLists = await Task.WhenAll(
						disallowedWebsitesFiles.Select(file => httpClient.GetStringAsync(file.DownloadUrl, cancellationToken))
					);

					_disallowedWebsites = disallowedWebsitesLists
						.SelectMany(disallowedWebsitesList => disallowedWebsitesList.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
						.Where(disallowedWebsite => !string.IsNullOrWhiteSpace(disallowedWebsite))
						.Distinct(StringComparer.InvariantCultureIgnoreCase)
						.ToImmutableHashSet(StringComparer.InvariantCultureIgnoreCase);

					// Disallowed words & phrases

					ImmutableList<GithubContent> disallowedWordsFiles = await githubClient.GetContentAsync(
						owner: safeSearchOptions.BadWordListOwner,
						repo: safeSearchOptions.BadWordListRepository,
						path: safeSearchOptions.DisallowedWordsPath,
						cancellationToken: cancellationToken);

					string[] disallowedWordsLists = await Task.WhenAll(
						disallowedWordsFiles.Select(file => httpClient.GetStringAsync(file.DownloadUrl, cancellationToken))
					);

					ImmutableList<string> disallowedWords = disallowedWordsLists
						.SelectMany(disallowedWordsList => disallowedWordsList.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
						.ToImmutableList();

					_disallowedWords = disallowedWords
						.Where(disallowedWord => !disallowedWord.Contains(' ', StringComparison.InvariantCultureIgnoreCase))
						.Distinct(StringComparer.InvariantCultureIgnoreCase)
						.ToImmutableHashSet(StringComparer.InvariantCultureIgnoreCase);

					_disallowedPhrases = disallowedWords
						.Where(disallowedWord => disallowedWord.Contains(' ', StringComparison.InvariantCultureIgnoreCase))
						.SelectMany(disallowedPhrase => {
							string[] wordsInPhrase = disallowedPhrase.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
							return wordsInPhrase.Select(wordInPhrase => new {
								Word = wordInPhrase,
								Phrase = disallowedPhrase
							});
						})
						.GroupBy(
							keySelector: wordInPhrase => wordInPhrase.Word,
							elementSelector: wordInPhrase => wordInPhrase.Phrase
						)
						.ToImmutableDictionary(
							keySelector: g => g.Key,
							elementSelector: g => g.Distinct(StringComparer.InvariantCultureIgnoreCase).ToImmutableHashSet(StringComparer.InvariantCultureIgnoreCase),
							keyComparer: StringComparer.InvariantCultureIgnoreCase
						);

					GC.Collect();
				}
			} finally {
				_semaphore.Release();
			}
		}
	}
}
