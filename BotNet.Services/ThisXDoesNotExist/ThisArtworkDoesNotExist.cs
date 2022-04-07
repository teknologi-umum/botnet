using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.ThisXDoesNotExist {
	public class ThisArtworkDoesNotExist {
		private readonly HttpClient _httpClient;

		public ThisArtworkDoesNotExist(
			HttpClient httpClient
		) {
			_httpClient = httpClient;
		}

		public async Task<byte[]> GetRandomArtworkAsync(CancellationToken cancellationToken) {
			const string url = "https://thisartworkdoesnotexist.com/";
			using HttpRequestMessage httpRequest = new(HttpMethod.Get, url);
			using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
			httpResponse.EnsureSuccessStatusCode();

			return await httpResponse.Content.ReadAsByteArrayAsync(cancellationToken);
		}
	}
}
