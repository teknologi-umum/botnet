using System;
using BotNet.Services.Tiktok;
using BotNet.Services.Twitter;
using FluentAssertions;
using Xunit;

namespace BotNet.Tests.Services.LinkSanitizers {
	public class RegexTests {
		[Theory]
		[InlineData("https://twitter.com/ShowwcaseHQ/status/1556259601829576707?t=S6GuFx37mAXOLI2wdusfXg&s=19", "https://twitter.com/ShowwcaseHQ/status/1556259601829576707")]
		[InlineData("WKWKWK alisnya Kevin https://twitter.com/ShowwcaseHQ/status/1556259601829576707?t=S6GuFx37mAXOLI2wdusfXg&s=19 😂", "https://twitter.com/ShowwcaseHQ/status/1556259601829576707")]
		[InlineData("https://twitter.com/ShowwcaseHQ/status/1556259601829576707", null)]
		public void CanSanitizeTwitterLinks(string url, string? cleaned) {
			if (TwitterLinkSanitizer.FindTrackedTwitterLink(url) is Uri trackedUrl) {
				Uri cleanedUrl = TwitterLinkSanitizer.Sanitize(trackedUrl);
				cleanedUrl.OriginalString.Should().Be(cleaned);
			} else {
				cleaned.Should().BeNull();
			}
		}

		[Theory]
		[InlineData("https://vt.tiktok.com/ZSR6XLMHh/?k=1", "https://vt.tiktok.com/ZSR6XLMHh/")]
		[InlineData("anjayyyyhttps://vt.tiktok.com/ZSR6XLMHh/?k=1", "https://vt.tiktok.com/ZSR6XLMHh/")]
		[InlineData("https://vt.tiktok.com/ZSR6XLMHh/", "https://vt.tiktok.com/ZSR6XLMHh/")]
		[InlineData("https://vt.tiktok.com/ZSR6XLMHh", "https://vt.tiktok.com/ZSR6XLMHh")]
		[InlineData("https://twitter.com/ShowwcaseHQ/status/1556259601829576707?t=S6GuFx37mAXOLI2wdusfXg&s=19", null)]
		public void CanDetectTrackedTiktokLinks(string url, string? trackedUrl) {
			if (TiktokLinkSanitizer.FindShortenedTiktokLink(url) is Uri shortenedTiktokLink) {
				shortenedTiktokLink.OriginalString.Should().Be(trackedUrl);
			} else {
				trackedUrl.Should().BeNull();
			}
		}
	}
}
