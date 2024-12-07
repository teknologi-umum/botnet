using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.FancyText {
	public static class FancyTextGenerator {
		private static readonly Dictionary<FancyTextStyle, ImmutableDictionary<char, string>> CharMapByStyle = new();
		private static readonly SemaphoreSlim Semaphore = new(1, 1);

		private static async Task<ImmutableDictionary<char, string>> GetCharMapAsync(FancyTextStyle style, CancellationToken cancellationToken) {
			await Semaphore.WaitAsync(cancellationToken);
			try {
				if (CharMapByStyle.TryGetValue(style, out ImmutableDictionary<char, string>? charMap)) {
					return charMap;
				}

				await using Stream resourceStream = typeof(FancyTextStyle).Assembly.GetManifestResourceStream($"BotNet.Services.FancyText.CharMaps.{style}.json")!;
				using StreamReader resourceStreamReader = new(resourceStream);
				string resourceText = await resourceStreamReader.ReadToEndAsync(cancellationToken);
				Dictionary<string, string> map = JsonSerializer.Deserialize<Dictionary<string, string>>(resourceText)!;
				charMap = map.ToImmutableDictionary(kvp => kvp.Key[0], kvp => kvp.Value);
				CharMapByStyle.Add(style, charMap);
				return charMap;
			} finally {
				Semaphore.Release();
			}
		}

		public static async Task<string> GenerateAsync(string text, FancyTextStyle style, CancellationToken cancellationToken) {
			ImmutableDictionary<char, string> charMap = await GetCharMapAsync(style, cancellationToken);
			StringBuilder fancyTextBuilder = new();
			foreach (char c in text) {
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
