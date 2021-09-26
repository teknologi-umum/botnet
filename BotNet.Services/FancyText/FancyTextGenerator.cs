using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.FancyText {
	public class FancyTextGenerator {
		private static readonly Dictionary<FancyTextStyle, ImmutableDictionary<char, string>> CHAR_MAP_BY_STYLE = new();
		private static readonly SemaphoreSlim SEMAPHORE = new(1, 1);

		private static async Task<ImmutableDictionary<char, string>> GetCharMapAsync(FancyTextStyle style, CancellationToken cancellationToken) {
			await SEMAPHORE.WaitAsync(cancellationToken);
			try {
				if (CHAR_MAP_BY_STYLE.TryGetValue(style, out ImmutableDictionary<char, string>? charMap)) {
					return charMap;
				}
				using Stream resourceStream = typeof(FancyTextStyle).Assembly.GetManifestResourceStream($"BotNet.Services.FancyText.CharMaps.{style}.json")!;
				using StreamReader resourceStreamReader = new(resourceStream);
				string resourceText = await resourceStreamReader.ReadToEndAsync();
				Dictionary<string, string> map = JsonSerializer.Deserialize<Dictionary<string, string>>(resourceText)!;
				charMap = map.ToImmutableDictionary(kvp => kvp.Key[0], kvp => kvp.Value);
				CHAR_MAP_BY_STYLE.Add(style, charMap!);
				return charMap;
			} catch (Exception exc) {
				throw;
			} finally {
				SEMAPHORE.Release();
			}
		}

		public static async Task<string> GenerateAsync(string text, FancyTextStyle style, CancellationToken cancellationToken) {
			ImmutableDictionary<char, string> charMap = await GetCharMapAsync(style, cancellationToken);
			StringBuilder fancyTextBuilder = new();
			foreach(char c in text) {
				if (charMap.TryGetValue(c, out string? replacement)) {
					fancyTextBuilder.Append(replacement);
				} else if (charMap.TryGetValue(char.ToLowerInvariant(c), out replacement)) {
					fancyTextBuilder.Append(replacement);
				} else {
					fancyTextBuilder.Append(c);
				}
			}
			return fancyTextBuilder.ToString();
		}
	}
}
