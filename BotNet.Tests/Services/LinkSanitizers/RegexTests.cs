using System;
using BotNet.Services.Tiktok;
using BotNet.Services.Twitter;
using BotNet.Services.Tokopedia;
using Shouldly;
using Xunit;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace BotNet.Tests.Services.LinkSanitizers {
	public class RegexTests {
		[Theory]
		[InlineData("https://twitter.com/ShowwcaseHQ/status/1556259601829576707?t=S6GuFx37mAXOLI2wdusfXg&s=19", "https://twitter.com/ShowwcaseHQ/status/1556259601829576707")]
		[InlineData("WKWKWK alisnya Kevin https://twitter.com/ShowwcaseHQ/status/1556259601829576707?t=S6GuFx37mAXOLI2wdusfXg&s=19 😂", "https://twitter.com/ShowwcaseHQ/status/1556259601829576707")]
		[InlineData("https://twitter.com/ShowwcaseHQ/status/1556259601829576707", null)]
		public void CanSanitizeTwitterLinks(string url, string? cleaned) {
			if (TwitterLinkSanitizer.FindTrackedTwitterLink(url) is Uri trackedUrl) {
				Uri cleanedUrl = TwitterLinkSanitizer.Sanitize(trackedUrl);
				cleanedUrl.OriginalString.ShouldBe(cleaned);
			} else {
				cleaned.ShouldBeNull();
			}
		}

		[Theory]
		[InlineData("https://x.com/ShowwcaseHQ/status/1556259601829576707?t=S6GuFx37mAXOLI2wdusfXg&s=19", "https://x.com/ShowwcaseHQ/status/1556259601829576707")]
		[InlineData("WKWKWK alisnya Kevin https://x.com/ShowwcaseHQ/status/1556259601829576707?t=S6GuFx37mAXOLI2wdusfXg&s=19 😂", "https://x.com/ShowwcaseHQ/status/1556259601829576707")]
		[InlineData("https://x.com/ShowwcaseHQ/status/1556259601829576707", null)]
		public void CanSanitizeXLinks(string url, string? cleaned) {
			if (XLinkSanitizer.FindTrackedXLink(url) is Uri trackedUrl) {
				Uri cleanedUrl = XLinkSanitizer.Sanitize(trackedUrl);
				cleanedUrl.OriginalString.ShouldBe(cleaned);
			} else {
				cleaned.ShouldBeNull();
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
				shortenedTiktokLink.OriginalString.ShouldBe(trackedUrl);
			} else {
				trackedUrl.ShouldBeNull();
			}
		}

		[Theory]
		[InlineData("https://tokopedia.link/S0B7SJVfLtb", "https://tokopedia.link/S0B7SJVfLtb")]
		[InlineData("https://tokopedia.link/EJPbFhHzbub", "https://tokopedia.link/EJPbFhHzbub")]
		[InlineData("https://tokopedia.link/HaDbM1iJStb", "https://tokopedia.link/HaDbM1iJStb")]
		[InlineData("awijdiwjdijtokhttps://tokopedia.link/HaDbM1iJStb", "https://tokopedia.link/HaDbM1iJStb")]
		[InlineData("https://www.tokopedia.com/tokomenulist/original-3d-wooden-puzzle-robotime-pendulum-clock-lk501", null)]
		public void CanDetectTokopediaShortenedLink(string url, string? trackedLink) {
			if (TokopediaLinkSanitizer.FindShortenedLink(url) is Uri shortenedLink) {
				shortenedLink.OriginalString.ShouldBe(trackedLink);
			} else {
				trackedLink.ShouldBeNull();
			}
		}

		[Theory(Skip = "Flaky test - tokopedia.link redirects expire and break CI")]
		[InlineData("https://tokopedia.link/S0B7SJVfLtb", "https://www.tokopedia.com/ceebstation/wrist-rest-keyboard-pinery-series-angled-tkl-by-patala")]
		public async Task CanSanitizeTokopediaTrackedLinkAsync(string trackedLink, string? cleanedLink) {
			using TokopediaLinkSanitizer sanitizer = new();
			using CancellationTokenSource cancellation = new();

			Uri trackedUrl = new(trackedLink);

			Uri cleaned = await sanitizer.SanitizeAsync(trackedUrl, cancellation.Token);
			cleaned.OriginalString.ShouldBe(cleanedLink);
		}

		[Fact(Skip = "Flaky test - depends on external HTTP requests to tokopedia.link")]
		public async Task InvalidTokopediaTrackedLinkAsync() {
			// Arange
			using TokopediaLinkSanitizer sanitizer = new();
			using CancellationTokenSource cancellation = new();

			Uri invalidUrl = new("https://tokopedia.link/S0B7SJVfL");

			// Act
			// Assert
			await Assert.ThrowsAsync<HttpRequestException>(async () => await sanitizer.SanitizeAsync(invalidUrl, cancellation.Token));
		}
	}
}
