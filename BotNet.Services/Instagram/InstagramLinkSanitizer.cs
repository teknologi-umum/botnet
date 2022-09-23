using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace BotNet.Services.Instagram {
	public class InstagramLinkSanitizer {
		public static Uri Sanitize(Uri link) {
			string sanitizedUri = link.GetLeftPart(UriPartial.Path);
			return new Uri(sanitizedUri);
		}

		public static Uri? FindTrackedInstagramLink(string message) {
			return Regex.Matches(message, "https://www.instagram.com/p/[0-9a-zA-Z-_]{8,16}/\\?")
				.Select(match => new Uri(match.Value))
				.FirstOrDefault()
				?? Regex.Matches(message, "https://www.instagram.com/reel/[0-9a-zA-Z-_]{8,16}/\\?")
					.Select(match => new Uri(match.Value))
					.FirstOrDefault();
		}
	}
}
