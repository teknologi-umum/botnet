using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.SafeSearch.Models;

namespace BotNet.Services.SafeSearch {
	public class SafeSearchDictionary {
		private static readonly char[] CONTENT_DELIMITERS = { ' ', '\t', '\r', '\n', '.', ',', ':', ';', '"' };
		private static readonly object DISALLOWED = new();
		private readonly SemaphoreSlim _semaphore = new(1, 1);
		private Trie<object>? _disallowedWebsites;
		private Trie<object>? _disallowedWords;
		private Trie<HashSet<string>>? _disallowedPhrases;

		public async Task<bool> IsUrlAllowedAsync(string url, CancellationToken cancellationToken) {
			await EnsureInitializedAsync(cancellationToken);
			return !Enumerable.Range(0, url.Length - 1)
				.Any(i => _disallowedWebsites!.ContainsKeyWhichIsTheBeginningOf(url[i..]));
		}

		public async Task<bool> IsContentAllowedAsync(string content, CancellationToken cancellationToken) {
			await EnsureInitializedAsync(cancellationToken);
			string[] words = content.Split(CONTENT_DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
			return !words.Any(word => {
				if (_disallowedWords!.ContainsKey(word)) return true;
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

					Trie<object> disallowedWebsites = new();
					foreach (string disallowedWebsitesResourceName in from resourceName in servicesAssembly.GetManifestResourceNames()
																	  where resourceName.StartsWith("BotNet.Services.SafeSearch.bad-word-list.websites.")
																	  select resourceName) {
						using Stream stream = servicesAssembly.GetManifestResourceStream(disallowedWebsitesResourceName)!;
						using StreamReader streamReader = new(stream);
						while (await streamReader.ReadLineAsync() is string { Length: > 0 } disallowedWebsite) {
							cancellationToken.ThrowIfCancellationRequested();
							disallowedWebsites.Add(disallowedWebsite, DISALLOWED);
						}
					}
					_disallowedWebsites = disallowedWebsites;

					Trie<object> disallowedWords = new();
					Trie<HashSet<string>> disallowedPhrases = new();
					foreach (string disallowedWordsResourceName in from resourceName in servicesAssembly.GetManifestResourceNames()
																   where resourceName.StartsWith("BotNet.Services.SafeSearch.bad-word-list.words.")
																   select resourceName) {
						using Stream stream = servicesAssembly.GetManifestResourceStream(disallowedWordsResourceName)!;
						using StreamReader streamReader = new(stream);
						while (await streamReader.ReadLineAsync() is string { Length: > 0 } disallowedWordOrPhrase) {
							if (disallowedWordOrPhrase.Contains(' ')) {
								foreach (string phraseWord in disallowedWordOrPhrase.Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
									if (disallowedPhrases.TryGetValue(phraseWord, out HashSet<string>? phrases)) {
										phrases.Add(disallowedWordOrPhrase);
									} else {
										disallowedPhrases.Add(phraseWord, new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { disallowedWordOrPhrase });
									}
								}
							} else {
								disallowedWords.Add(disallowedWordOrPhrase, DISALLOWED);
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
