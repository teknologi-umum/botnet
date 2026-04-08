using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Downdetector;
using Shouldly;
using Xunit;

namespace BotNet.Tests.Services.Downdetector {
	public class DowndetectorClientTests {
		[Fact]
		public async Task CheckServicesAsync_ReturnsResults() {
			// Arrange
			using System.Net.Http.HttpClient httpClient = new();
			DowndetectorClient client = new(httpClient);
			
			// Act
			System.Collections.Generic.List<DowndetectorServiceStatus> results = await client.CheckServicesAsync(CancellationToken.None);
			
			// Assert
			results.ShouldNotBeNull();
			results.Count.ShouldBeGreaterThan(0);
			results.ShouldAllBe(r => r.ServiceName != null);
		}
		
		[Fact]
		public async Task CheckServicesAsync_ReturnsExpectedServices() {
			// Arrange
			using System.Net.Http.HttpClient httpClient = new();
			DowndetectorClient client = new(httpClient);
			
			// Act
			System.Collections.Generic.List<DowndetectorServiceStatus> results = await client.CheckServicesAsync(CancellationToken.None);
			
			// Assert
			results.ShouldContain(r => r.ServiceName == "Google");
			results.ShouldContain(r => r.ServiceName == "Facebook");
			results.ShouldContain(r => r.ServiceName == "YouTube");
		}
		
		[Fact]
		public async Task CheckServicesAsync_HasStatusInformation() {
			// Arrange
			using System.Net.Http.HttpClient httpClient = new();
			DowndetectorClient client = new(httpClient);
			
			// Act
			System.Collections.Generic.List<DowndetectorServiceStatus> results = await client.CheckServicesAsync(CancellationToken.None);
			
			// Assert
			results.ShouldAllBe(r => r.Description != null);
			// HasIssues can be null if fetch failed, but should be bool for successful checks
		}
	}
}
