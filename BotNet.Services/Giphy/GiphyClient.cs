using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Giphy.Models;
using BotNet.Services.Json;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Giphy {
	[Obsolete("Use TenorClient instead.")]
	[ExcludeFromCodeCoverage]
	public class GiphyClient {
		private const string GifSearchEndpoint = "https://api.giphy.com/v1/gifs/search";
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;
		private readonly JsonSerializerOptions _jsonSerializerOptions;

		public GiphyClient(
			HttpClient httpClient,
			IOptions<GiphyOptions> giphyOptionsAccessor
		) {
			GiphyOptions giphyOptions = giphyOptionsAccessor.Value;
			_apiKey = giphyOptions.ApiKey ?? throw new InvalidOperationException("Giphy api key not configured. Please add a .NET secret with key 'GiphyOptions:ApiKey' or a Docker secret with key 'GiphyOptions__ApiKey'");
			_httpClient = httpClient;
			_jsonSerializerOptions = new JsonSerializerOptions {
				PropertyNamingPolicy = new SnakeCaseNamingPolicy()
			};
		}

		public async Task<GifObject[]> SearchGifsAsync(string query, CancellationToken cancellationToken) {
			string requestUrl = $"{GifSearchEndpoint}?api_key={_apiKey}&q={WebUtility.UrlEncode(query)}&offset=0&limit=25&rating=g&lang=id";
			GifSearchResult? searchResult = await _httpClient.GetFromJsonAsync<GifSearchResult>(requestUrl, _jsonSerializerOptions, cancellationToken);
			return searchResult?.Data ?? [];
		}
	}
}
