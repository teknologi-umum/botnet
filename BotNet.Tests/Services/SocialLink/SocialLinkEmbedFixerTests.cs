using System;
using System.Collections.Generic;
using System.Linq;
using BotNet.Services.SocialLink;
using FluentAssertions;
using Xunit;

namespace BotNet.Tests.Services.SocialLink {
	public class SocialLinkEmbedFixerTests {
		[Theory]
		[InlineData("https://www.instagram.com/reel/C0XXKVnpRUI/", "https://www.ddinstagram.com/reel/C0XXKVnpRUI/")]
		[InlineData("https://instagram.com/reel/C0XXKVnpRUI/", "https://ddinstagram.com/reel/C0XXKVnpRUI/")]
		[InlineData("https://twitter.com/ShowwcaseHQ/status/1556259601829576707?t=S6GuFx37mAXOLI2wdusfXg&s=19",
			"https://vxtwitter.com/ShowwcaseHQ/status/1556259601829576707?t=S6GuFx37mAXOLI2wdusfXg&s=19")]
		[InlineData("https://www.twitter.com/ShowwcaseHQ/status/1556259601829576707?t=S6GuFx37mAXOLI2wdusfXg&s=19",
			"https://www.vxtwitter.com/ShowwcaseHQ/status/1556259601829576707?t=S6GuFx37mAXOLI2wdusfXg&s=19")]
		public void CanReplaceSocialLink(string url, string? replacedUrl) {
			Uri resUrl = SocialLinkEmbedFixer.Fix(new Uri(url));
			resUrl.OriginalString.Should().Be(replacedUrl);
		}

		[Theory]
		[InlineData("LUCU ga si? https://www.instagram.com/reel/C0XXKVnpRUI/",
			new[] { "https://www.instagram.com/reel/C0XXKVnpRUI/" })]
		[InlineData("LUCU ga si? https://instagram.com/reel/C0XXKVnpRUI/",
			new[] { "https://instagram.com/reel/C0XXKVnpRUI/" })]
		[InlineData(
			"WKWKWK alisnya Kevin https://twitter.com/ShowwcaseHQ/status/1556259601829576707?t=S6GuFx37mAXOLI2wdusfXg&s=19 😂",
			new[] { "https://twitter.com/ShowwcaseHQ/status/1556259601829576707" })]
		[InlineData(
			"WKWKWK alisnya Kevin https://x.com/ShowwcaseHQ/status/1556259601829576707?t=S6GuFx37mAXOLI2wdusfXg&s=19 😂",
			new[] { "https://x.com/ShowwcaseHQ/status/1556259601829576707" })]
		[InlineData("https://www.x.com/ShowwcaseHQ/status/1556259601829576707",
			new[] { "https://www.x.com/ShowwcaseHQ/status/1556259601829576707" })]
		[InlineData("https://www.twitter.com/ShowwcaseHQ/status/1556259601829576707",
			new[] { "https://www.twitter.com/ShowwcaseHQ/status/1556259601829576707" })]
		[InlineData(
			"lo udah liat ini belum? https://www.twitter.com/ShowwcaseHQ/status/1556259601829576707. Ohh iya, ini juga lucu: https://instagram.com/reel/C0XXKVnpRUI",
			new[] { "https://www.twitter.com/ShowwcaseHQ/status/1556259601829576707", "https://instagram.com/reel/C0XXKVnpRUI" })]
		[InlineData("Iyakah?", new string[] { })]
		public void CanDetectSocialLink(string message, IEnumerable<string>? urls) {
			List<string> resUrls =
				SocialLinkEmbedFixer.GetPossibleUrls(message)
					.Select(u => u.OriginalString).ToList();
			if (urls == null) {
				resUrls.Should().BeNull();
			} else {
				resUrls.Should().BeEquivalentTo(urls);
			}
		}
	}
}
