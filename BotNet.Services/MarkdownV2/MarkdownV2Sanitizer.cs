using System.Collections.Generic;
using System.Text;

namespace BotNet.Services.MarkdownV2 {
	public static class MarkdownV2Sanitizer {
		private static readonly HashSet<char> CHARACTERS_TO_ESCAPE = [
			'[', ']', '(', ')', '~', '>', '#',
			'+', '-', '=', '|', '{', '}', '.', '!'
		];

		public static string Sanitize(string input) {
			if (string.IsNullOrEmpty(input))
				return input;

			// Use StringBuilder for efficient string manipulation
			StringBuilder sanitized = new(input.Length);

			char previousCharacter = '\0';
			foreach (char character in input) {
				// If the character is in our list, append a backslash before it
				if (CHARACTERS_TO_ESCAPE.Contains(character)
					&& previousCharacter != '\\') {
					sanitized.Append('\\');
				}
				sanitized.Append(character);
				previousCharacter = character;
			}

			return sanitized.ToString();
		}

		private static int CountOccurences(string text, string substring) {
			int count = 0;
			int i = 0;

			while ((i = text.IndexOf(substring, i)) != -1) {
				i += substring.Length;
				count++;
			}

			return count;
		}
	}
}
