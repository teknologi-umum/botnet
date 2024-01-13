using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BotNet.Services.SocialLink {
	public partial class SocialLinkEmbedFixer {

		/// <summary>
		/// Fixes the given URI by replacing specific host names with new host names.
		/// </summary>
		/// <param name="link">The original URI to be fixed.</param>
		/// <returns>A new URI with the host names replaced.</returns>
		public static Uri Fix(Uri link) {
			string url = link.ToString();
			string newUrl = link.Host switch {
				"twitter.com" => url.Replace("//twitter.com/", "//vxtwitter.com/"),
				"www.twitter.com" => url.Replace("//www.twitter.com/", "//www.vxtwitter.com/"),
				"x.com" => url.Replace("//x.com/", "//vxtwitter.com/"),
				"www.x.com" => url.Replace("//www.x.com/", "//www.vxtwitter.com/"),
				"instagram.com" => url.Replace("//instagram.com/", "//ddinstagram.com/"),
				"www.instagram.com" => url.Replace("//www.instagram.com/", "//www.ddinstagram.com/"),
				_ => url
			};
			return new Uri(newUrl);
		}

		/// <summary>
		/// Extracts possible URLs from the given message that match Twitter and Instagram patterns.
		/// </summary>
		/// <param name="message">The message to search for URLs.</param>
		/// <returns>An enumerable collection of URLs.</returns>
		public static IEnumerable<Uri> GetPossibleUrls(string message) {
			MatchCollection twitterMatches = TwitterRegex().Matches(message);
			foreach (Match match in twitterMatches) {
				yield return new Uri(match.Value);
			}

			MatchCollection instagramMatches = InstagramRegex().Matches(message);
			foreach (Match match in instagramMatches) {
				yield return new Uri(match.Value);
			}
		}

		[GeneratedRegex(@"(?:https?:\/\/)?(?:www.)?(?:twitter\.com|x\.com)\/(?:[a-zA-Z0-9_]+\/status\/)?[0-9]+")]
		private static partial Regex TwitterRegex();

		[GeneratedRegex(@"(?:https?:\/\/)?(?:www.)?instagram.com\/?([a-zA-Z0-9\._\-]+)?\/([p]+)?([reel]+)?([tv]+)?([stories]+)?\/([a-zA-Z0-9\-_\.]+)\/?([0-9]+)?")]
		private static partial Regex InstagramRegex();
	}
}
