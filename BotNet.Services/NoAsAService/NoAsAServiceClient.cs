using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.NoAsAService {
	public sealed class NoAsAServiceClient(
		HttpClient httpClient
	) {
		public async Task<string> GetNoReasonAsync(CancellationToken cancellationToken) {
			NoResponse? response = await httpClient.GetFromJsonAsync<NoResponse>(
				requestUri: "https://naas.isalman.dev/no",
				cancellationToken: cancellationToken
			);

			return response?.Reason ?? "No.";
		}

		private sealed record NoResponse(string Reason);
	}
}
