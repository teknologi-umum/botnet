using System;
using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.PSE.JsonModels;
using Microsoft.Extensions.Logging;

namespace BotNet.Services.PSE {
	public class PSEClient {
		private const string BASE_URL = "https://pse.komdigi.go.id/api/v1/tdpse/tdpse-list";
		private readonly HttpClient _httpClient;
		private readonly ILogger<PSEClient> _logger;

		public PSEClient(
			HttpClient httpClient,
			ILogger<PSEClient> logger
		) {
			_httpClient = httpClient;
			_logger = logger;
			_httpClient.DefaultRequestHeaders.Add("Origin", "https://pse.komdigi.go.id");
			_httpClient.DefaultRequestHeaders.Add("Referer", "https://pse.komdigi.go.id/pse");
		}

		public async Task<(
			ImmutableList<DigitalService> DigitalServices,
			int TotalRows
		)> SearchAsync(
			string keyword,
			int length,
			int start,
			CancellationToken cancellationToken
		) {
			using CancellationTokenSource timeoutSource = new(TimeSpan.FromSeconds(10));
			using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken);
			cancellationToken = linkedSource.Token;

			object requestBody = new {
				keyword = keyword,
				length = length,
				start = start,
				category = "terdaftar"
			};

			_logger.LogInformation("POST {Url} with keyword={Keyword}, length={Length}, start={Start}", BASE_URL, keyword, length, start);

			using HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(
				requestUri: BASE_URL,
				value: requestBody,
				cancellationToken: cancellationToken
			);
			httpResponse.EnsureSuccessStatusCode();
			string json = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
			DigitalServicesResponse? response = JsonSerializer.Deserialize<DigitalServicesResponse>(json);

			if (response is null || response.Status != "success") {
				throw new HttpRequestException($"API returned error: {response?.Error ?? "Unknown error"}");
			}

			return (
				DigitalServices: response.Data.Data,
				TotalRows: response.Data.TotalRows
			);
		}
	}
}
