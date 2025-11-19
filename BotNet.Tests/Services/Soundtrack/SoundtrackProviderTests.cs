using BotNet.Services.Soundtrack;
using Shouldly;
using Xunit;

namespace BotNet.Tests.Services.Soundtrack {
	public class SoundtrackProviderTests {
		private readonly SoundtrackProvider _soundtrackProvider = new();

		[Fact]
		public void GetRandomPicks_ReturnsTwoDifferentSites() {
			// Act
			(SoundtrackSite first, SoundtrackSite second) = _soundtrackProvider.GetRandomPicks();

			// Assert
			first.ShouldNotBeNull();
			second.ShouldNotBeNull();
			first.ShouldNotBe(second);
		}

		[Fact]
		public void GetRandomPicks_ReturnsValidUrls() {
			// Act
			(SoundtrackSite first, SoundtrackSite second) = _soundtrackProvider.GetRandomPicks();

			// Assert
			first.Url.ShouldStartWith("http");
			second.Url.ShouldStartWith("http");
			first.Name.ShouldNotBeNullOrWhiteSpace();
			second.Name.ShouldNotBeNullOrWhiteSpace();
		}

		[Fact]
		public void GetRandomPicks_ReturnsDifferentResultsAcrossMultipleCalls() {
			// Act
			(SoundtrackSite first1, SoundtrackSite second1) = _soundtrackProvider.GetRandomPicks();
			(SoundtrackSite first2, SoundtrackSite second2) = _soundtrackProvider.GetRandomPicks();
			(SoundtrackSite first3, SoundtrackSite second3) = _soundtrackProvider.GetRandomPicks();

			// Assert - at least one should be different across 3 calls
			bool hasDifference = 
				first1 != first2 || 
				first1 != first3 || 
				second1 != second2 || 
				second1 != second3;
			
			hasDifference.ShouldBeTrue();
		}
	}
}
