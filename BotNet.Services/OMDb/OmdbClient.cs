using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace BotNet.Services.OMDb {
	public sealed class OmdbClient(
		HttpClient httpClient,
		IOptions<OmdbOptions> omdbOptions
	) {
		private readonly string _apiKey = omdbOptions.Value.ApiKey;

		public async Task<OmdbResponse> GetByTitleAsync(string title, CancellationToken cancellationToken) {
			string url = $"http://www.omdbapi.com/?apikey={_apiKey}&t={Uri.EscapeDataString(title)}&plot=full";

			using HttpRequestMessage request = new(HttpMethod.Get, url);
			using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			string json = await response.Content.ReadAsStringAsync(cancellationToken);
			OmdbResponse? omdbResponse = JsonSerializer.Deserialize<OmdbResponse>(json);

			if (omdbResponse == null) {
				throw new InvalidOperationException("Failed to deserialize OMDb API response");
			}

			if (omdbResponse.Response == "False") {
				throw new InvalidOperationException(omdbResponse.Error ?? "Movie not found");
			}

			return omdbResponse;
		}

		public async Task<byte[]> GetPosterImageAsync(string posterUrl, CancellationToken cancellationToken) {
			if (string.IsNullOrWhiteSpace(posterUrl) || posterUrl == "N/A") {
				throw new InvalidOperationException("No poster available for this title");
			}

			return await httpClient.GetByteArrayAsync(posterUrl, cancellationToken);
		}
	}
}
