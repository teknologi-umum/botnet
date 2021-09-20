using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace BotNet.Tests.TestUtilities {
	public class HttpClientMock {
		public static async Task TestHttpClientUsingDummyContentAsync(string content, Func<HttpClient, Task> testAsync) {
			Mock<HttpMessageHandler> handlerMock = new();

			using HttpResponseMessage responseMessage = new() {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(content)
			};

			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync(responseMessage);

			using HttpClient httpClient = new(handlerMock.Object);

			await testAsync(httpClient);
		}
	}
}
