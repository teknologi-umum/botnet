using System;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.MemoryPressureCoordinator;
using BotNet.Services.SafeSearch;
using FluentAssertions;
using Xunit;

namespace BotNet.Tests.Services.SafeSearch {
	public class SafeSearchDictionaryTests {
		[Fact]
		public async Task CanBuildDictionaryAndCheckContentAsync() {
			MemoryPressureSemaphore memoryPressureSemaphore = new();

			long startingAllocation = GC.GetTotalAllocatedBytes(true);
			SafeSearchDictionary safeSearchDictionary = new(memoryPressureSemaphore);
			bool isAllowed = await safeSearchDictionary.IsUrlAllowedAsync("https://www.apple.com/", CancellationToken.None);
			long finalAllocation = GC.GetTotalAllocatedBytes(true);
			isAllowed.Should().BeTrue();
			long allocatedForDictionary = finalAllocation - startingAllocation;
			allocatedForDictionary.Should().BeLessThan(100_000_000);

			isAllowed = await safeSearchDictionary.IsUrlAllowedAsync("https://www.pornhub.com/", CancellationToken.None);
			isAllowed.Should().BeFalse();

			isAllowed = await safeSearchDictionary.IsUrlAllowedAsync("http://www.pornhub.com/", CancellationToken.None);
			isAllowed.Should().BeFalse();

			isAllowed = await safeSearchDictionary.IsUrlAllowedAsync("www.pornhub.com", CancellationToken.None);
			isAllowed.Should().BeFalse();

			isAllowed = await safeSearchDictionary.IsContentAllowedAsync("Lorem ipsum dolor sit amet", CancellationToken.None);
			isAllowed.Should().BeTrue();

			isAllowed = await safeSearchDictionary.IsContentAllowedAsync("Lorem ipsum dolor porn amet", CancellationToken.None);
			isAllowed.Should().BeFalse();

			isAllowed = await safeSearchDictionary.IsContentAllowedAsync("Cerita lorem ipsum dolor sit hot amet", CancellationToken.None);
			isAllowed.Should().BeFalse();

			isAllowed = await safeSearchDictionary.IsContentAllowedAsync("Cerita lorem ipsum dolor sit amet", CancellationToken.None);
			isAllowed.Should().BeTrue();
		}
	}
}
