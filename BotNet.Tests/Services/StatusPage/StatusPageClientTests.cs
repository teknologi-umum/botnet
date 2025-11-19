using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.StatusPage;
using Moq;
using Moq.Protected;
using Shouldly;
using Xunit;

namespace BotNet.Tests.Services.StatusPage {
	public class StatusPageClientTests {
		[Fact]
		public async Task CheckAllServicesAsync_IncludesMetaServices() {
			// Arrange
			Mock<HttpMessageHandler> handlerMock = new();
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync((HttpRequestMessage req, CancellationToken ct) => {
					// Mock response for different services
					if (req.RequestUri?.AbsoluteUri.Contains("/api/v2/status.json") == true) {
						// Standard Statuspage.io format
						return new HttpResponseMessage {
							StatusCode = HttpStatusCode.OK,
							Content = new StringContent("{\"status\":{\"indicator\":\"none\",\"description\":\"All Systems Operational\"}}")
						};
					} else if (req.RequestUri?.AbsoluteUri.Contains("slack.com") == true) {
						// Slack format
						return new HttpResponseMessage {
							StatusCode = HttpStatusCode.OK,
							Content = new StringContent("{\"status\":\"ok\",\"active_incidents\":[]}")
						};
					} else if (req.RequestUri?.Host.Contains("facebook.com") == true ||
					           req.RequestUri?.Host.Contains("instagram.com") == true ||
					           req.RequestUri?.Host.Contains("whatsapp.com") == true ||
					           req.RequestUri?.Host.Contains("threads.net") == true) {
						// Meta services - HTTP availability check
						return new HttpResponseMessage {
							StatusCode = HttpStatusCode.OK
						};
					}
					
					return new HttpResponseMessage {
						StatusCode = HttpStatusCode.NotFound
					};
				});

			using HttpClient httpClient = new(handlerMock.Object);
			StatusPageClient statusPageClient = new(httpClient);

			// Act
			System.Collections.Generic.List<ServiceStatus> results = await statusPageClient.CheckAllServicesAsync(CancellationToken.None);

			// Assert
			results.ShouldNotBeEmpty();
			
			ServiceStatus? facebook = results.FirstOrDefault(s => s.ServiceName == "Facebook");
			facebook.ShouldNotBeNull();
			facebook.IsOperational.ShouldBeTrue();
			
			ServiceStatus? instagram = results.FirstOrDefault(s => s.ServiceName == "Instagram");
			instagram.ShouldNotBeNull();
			instagram.IsOperational.ShouldBeTrue();
			
			ServiceStatus? whatsApp = results.FirstOrDefault(s => s.ServiceName == "WhatsApp");
			whatsApp.ShouldNotBeNull();
			whatsApp.IsOperational.ShouldBeTrue();
			
			ServiceStatus? threads = results.FirstOrDefault(s => s.ServiceName == "Threads");
			threads.ShouldNotBeNull();
			threads.IsOperational.ShouldBeTrue();
		}

		[Fact]
		public async Task CheckAllServicesAsync_ReturnsExpectedServiceCount() {
			// Arrange
			Mock<HttpMessageHandler> handlerMock = new();
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync((HttpRequestMessage req, CancellationToken ct) => {
					// Mock response for all services
					if (req.RequestUri?.AbsoluteUri.Contains("/api/v2/status.json") == true) {
						return new HttpResponseMessage {
							StatusCode = HttpStatusCode.OK,
							Content = new StringContent("{\"status\":{\"indicator\":\"none\",\"description\":\"All Systems Operational\"}}")
						};
					} else if (req.RequestUri?.AbsoluteUri.Contains("slack.com") == true) {
						return new HttpResponseMessage {
							StatusCode = HttpStatusCode.OK,
							Content = new StringContent("{\"status\":\"ok\",\"active_incidents\":[]}")
						};
					} else {
						// Meta services
						return new HttpResponseMessage {
							StatusCode = HttpStatusCode.OK
						};
					}
				});

			using HttpClient httpClient = new(handlerMock.Object);
			StatusPageClient statusPageClient = new(httpClient);

			// Act
			System.Collections.Generic.List<ServiceStatus> results = await statusPageClient.CheckAllServicesAsync(CancellationToken.None);

			// Assert - Should have 22 standard services + 1 Slack + 4 Meta services = 27 total
			results.Count.ShouldBe(27);
		}
	}
}
