using System;
using BotNet.Services.Preview;
using Shouldly;
using Xunit;

namespace BotNet.Tests.Services.Preview {
	public class RegexTests {

	[Theory]
	[InlineData("https://www.youtube.com/watch?v=wUGbUERmhJM&t=2711s", "https://www.youtube.com/watch?v=wUGbUERmhJM")]
	[InlineData("https://www.youtube.com/watch?v=L8JJernNrS8", "https://www.youtube.com/watch?v=L8JJernNrS8")]
	[InlineData("https://www.youtube.com/watch?v=JdqL89ZZwFw", "https://www.youtube.com/watch?v=JdqL89ZZwFw")]
	public void YoutubeLink(string url, string validLink) {
		Uri? uri = YoutubePreview.ValidateYoutubeLink(url);
		uri?.OriginalString.ShouldBe(validLink);
	}

	[Theory]
	[InlineData("https://www.youtube.com/v=wUGbUERmhJM&t=2711s", null)]
	[InlineData("https://www.youtube.com/?t=2711s", null)]
	[InlineData("http://www.example.com", null)]
	public void InvalidYoutubeLink(string url, string? validLink) {
		Uri? uri = YoutubePreview.ValidateYoutubeLink(url);
		uri.ShouldBeNull(validLink);
	}

	[Theory]
	[InlineData("Tonton ini https://www.youtube.com/watch?v=wUGbUERmhJM&t=2711s", "https://www.youtube.com/watch?v=wUGbUERmhJM")]
	[InlineData("https://www.youtube.com/watch?v=wUGbUERmhJM&t=2711s cocok", "https://www.youtube.com/watch?v=wUGbUERmhJM")]
	[InlineData("http://www.example.com https://www.youtube.com/watch?v=wUGbUERmhJM&t=2711s", "https://www.youtube.com/watch?v=wUGbUERmhJM")]
	public void LinkWithMessage(string url, string validLink) {
		Uri? uri = YoutubePreview.ValidateYoutubeLink(url);
		uri?.OriginalString.ShouldBe(validLink);
	}		[Theory]
		[InlineData("Tonton ini https://www.youtube.com/v=wUGbUERmhJM&t=2711s", null)]
		[InlineData("https://www.youtube.com/?t=2711s cocok", null)]
		[InlineData("http://www.example.com salah link", null)]
		public void InvalidLinkWithMessage(string url, string? validLink) {
			Uri? uri = YoutubePreview.ValidateYoutubeLink(url);
			uri.ShouldBeNull(validLink);
		}
	}
}
