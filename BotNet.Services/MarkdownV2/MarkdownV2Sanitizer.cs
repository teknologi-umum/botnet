using System.Collections.Generic;
using System.Text;

namespace BotNet.Services.MarkdownV2 {
	public static class MarkdownV2Sanitizer {
		private static readonly HashSet<char> CHARACTERS_TO_ESCAPE = new() {
			'_', '*', '[', ']', '(', ')', '~', '>', '#',
			'+', '-', '=', '|', '{', '}', '.', '!'
		};

		public static string Sanitize(string input) {
			if (string.IsNullOrEmpty(input))
				return input;

			// Use StringBuilder for efficient string manipulation
			StringBuilder sanitized = new(input.Length);

			foreach (char character in input) {
				// If the character is in our list, append a backslash before it
				if (CHARACTERS_TO_ESCAPE.Contains(character)) {
					sanitized.Append('\\');
				}
				sanitized.Append(character);
			}

			return sanitized.ToString();
		}
	}
}
