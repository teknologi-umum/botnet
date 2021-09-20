using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.SafeSearch {
	public class SafeSearchDictionary {
		private static readonly char[] CONTENT_DELIMITERS = { ' ', '\t', '\r', '\n', '.', ',', ':', ';', '"' };
		private static readonly object DISALLOWED = new();
		private readonly SemaphoreSlim _semaphore = new(1, 1);
		private HashSet<string>? _disallowedWebsites;
		private HashSet<string>? _disallowedWords;
		private Dictionary<string, HashSet<string>>? _disallowedPhrases;

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
				if (_disallowedPhrases!.TryGetValue(word, out HashSet<string>? phrases)) {
					foreach (string phrase in phrases) {
						string[] phraseWords = phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);
						if (phraseWords.All(phraseWord => words.Contains(phraseWord, StringComparer.InvariantCultureIgnoreCase))) return true;
					}
				}
				return false;
			});
		}

		private async Task EnsureInitializedAsync(CancellationToken cancellationToken) {
			await _semaphore.WaitAsync(cancellationToken);
			try {
				if (_disallowedWebsites == null
					|| _disallowedWords == null
					|| _disallowedPhrases == null) {
					Assembly servicesAssembly = Assembly.GetAssembly(typeof(SafeSearchDictionary))!;

					HashSet<string> disallowedWebsites = new(StringComparer.InvariantCultureIgnoreCase);
					foreach (string disallowedWebsitesResourceName in from resourceName in servicesAssembly.GetManifestResourceNames()
																	  where resourceName.StartsWith("BotNet.Services.SafeSearch.bad_word_list.websites.")
																	  select resourceName) {
						using Stream stream = servicesAssembly.GetManifestResourceStream(disallowedWebsitesResourceName)!;
						using StreamReader streamReader = new(stream);
						while (await streamReader.ReadLineAsync() is string disallowedWebsite) {
							cancellationToken.ThrowIfCancellationRequested();
							if (!string.IsNullOrWhiteSpace(disallowedWebsite)) {
								disallowedWebsites.Add(disallowedWebsite);
							}
						}
					}
					_disallowedWebsites = disallowedWebsites;

					HashSet<string> disallowedWords = new(StringComparer.InvariantCultureIgnoreCase);
					Dictionary<string, HashSet<string>> disallowedPhrases = new(StringComparer.InvariantCultureIgnoreCase);
					foreach (string disallowedWordsResourceName in from resourceName in servicesAssembly.GetManifestResourceNames()
																   where resourceName.StartsWith("BotNet.Services.SafeSearch.bad_word_list.words.")
																   select resourceName) {
						using Stream stream = servicesAssembly.GetManifestResourceStream(disallowedWordsResourceName)!;
						using StreamReader streamReader = new(stream);
						while (await streamReader.ReadLineAsync() is string disallowedWordOrPhrase) {
							cancellationToken.ThrowIfCancellationRequested();
							if (disallowedWordOrPhrase.Contains(' ')) {
								foreach (string phraseWord in disallowedWordOrPhrase.Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
									if (disallowedPhrases.TryGetValue(phraseWord, out HashSet<string>? phrases)) {
										phrases.Add(disallowedWordOrPhrase);
									} else {
										disallowedPhrases.Add(phraseWord, new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { disallowedWordOrPhrase });
									}
								}
							} else {
								disallowedWords.Add(disallowedWordOrPhrase);
							}
						}
					}
					_disallowedWords = disallowedWords;
					_disallowedPhrases = disallowedPhrases;
				}
			} finally {
				_semaphore.Release();
			}
		}
	}
}
