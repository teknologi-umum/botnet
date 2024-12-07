using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.ThisXDoesNotExist {
	public class ThisCatDoesNotExist(
		HttpClient httpClient
	) {
		public async Task<byte[]> GetRandomCatImageAsync(CancellationToken cancellationToken) {
			const string url = "https://thiscatdoesnotexist.com/";
			using HttpRequestMessage httpRequest = new(HttpMethod.Get, url);
			using HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
			httpResponse.EnsureSuccessStatusCode();

			return await httpResponse.Content.ReadAsByteArrayAsync(cancellationToken);
		}
	}
}
