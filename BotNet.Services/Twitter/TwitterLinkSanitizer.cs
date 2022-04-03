using System;
using System.Text.RegularExpressions;

namespace BotNet.Services.Twitter {
	public class TwitterLinkSanitizer {
		public static Uri Sanitize(Uri link) {
			string sanitizedUri = link.GetLeftPart(UriPartial.Path);
			return new Uri(sanitizedUri);
		}

		public static bool IsTwitterLink(Uri link) {
			return Regex.IsMatch(link.OriginalString, "^https://twitter.com/[0-9a-zA-Z_]+/status/[0-9]{18,20}");
		}

		public static bool IsTrackedTwitterLink(Uri link) {
			return Regex.IsMatch(link.OriginalString, "^https://twitter.com/[0-9a-zA-Z_]+/status/[0-9]{18,20}\\?");
		}
	}
}
