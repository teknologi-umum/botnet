using System;
using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.PSE.Models;

namespace BotNet.Services.PSE {
	public class PSEClient {
		private const string BASE_URL = "https://pse.kominfo.go.id/static/json-static";
		private readonly HttpClient _httpClient;

		public PSEClient(
			HttpClient httpClient
		) {
			_httpClient = httpClient;
		}

		public async Task<DateTime> GetLastGeneratedAsync(CancellationToken cancellationToken) {
			TimestampsResponse? response = await _httpClient.GetFromJsonAsync<TimestampsResponse>($"{BASE_URL}/generationInfo.json", cancellationToken);

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
			DigitalServicesResponse? response = await _httpClient.GetFromJsonAsync<DigitalServicesResponse>(
				requestUri: $"{BASE_URL}/{domicile.ToPSEDomicile()}_{status.ToPSEStatus()}/{page + 1}.json",
				cancellationToken: cancellationToken
			);

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
