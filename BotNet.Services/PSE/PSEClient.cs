using System;
using System.Collections.Immutable;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.PSE.JsonModels;
using Microsoft.Extensions.Logging;

namespace BotNet.Services.PSE {
	public class PSEClient {
		private const string BASE_URL = "https://pse.kominfo.go.id/static/json-static";
		private readonly HttpClient _httpClient;
		private readonly ILogger<PSEClient> _logger;

		public PSEClient(
			HttpClient httpClient,
			ILogger<PSEClient> logger
		) {
			_httpClient = httpClient;
			_logger = logger;
		}

		public async Task<DateTime> GetLastGeneratedAsync(CancellationToken cancellationToken) {
			using CancellationTokenSource timeoutSource = new(TimeSpan.FromSeconds(10));
			using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken);
			cancellationToken = linkedSource.Token;

			string url = $"{BASE_URL}/generationInfo.json";
			_logger.LogInformation("GET {0}", url);
			using HttpResponseMessage httpResponse = await _httpClient.GetAsync(
				requestUri: url,
				cancellationToken: cancellationToken
			);
			httpResponse.EnsureSuccessStatusCode();
			string json = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
			TimestampsResponse? response = JsonSerializer.Deserialize<TimestampsResponse>(json);

			if (response is null) {
				throw new HttpRequestException();
			}

			return response.Timestamps.LastGenerated;
		}

		public async Task<(
			ImmutableList<DigitalService> DigitalServices,
			PaginationMetadata PaginationMetadata
		)> GetDigitalServicesAsync(
			Domicile domicile,
			Status status,
			int page,
			CancellationToken cancellationToken
		) {
			using CancellationTokenSource timeoutSource = new(TimeSpan.FromSeconds(5));
			using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken);
			cancellationToken = linkedSource.Token;

			string url = $"{BASE_URL}/{domicile.ToPSEDomicile()}_{status.ToPSEStatus()}/{page - 1}.json";
			_logger.LogInformation("GET {0}", url);

			using HttpResponseMessage httpResponse = await _httpClient.GetAsync(
				requestUri: url,
				cancellationToken: cancellationToken
			);
			httpResponse.EnsureSuccessStatusCode();
			string json = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

			if (json.StartsWith("<!doctype html>", StringComparison.OrdinalIgnoreCase)) {
				return (
					DigitalServices: ImmutableList<DigitalService>.Empty,
					PaginationMetadata: new PaginationMetadata(1, 1, 0, 0, 10, 0)
				);
			}

			DigitalServicesResponse? response = JsonSerializer.Deserialize<DigitalServicesResponse>(json);

			if (response is null) {
				throw new HttpRequestException();
			}

			return (
				DigitalServices: response.DigitalServices,
				PaginationMetadata: response.Metadata.PaginationMetadata
			);
		}
	}
}
