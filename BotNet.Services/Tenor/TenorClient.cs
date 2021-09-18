using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Json;
using BotNet.Services.Tenor.Models;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Tenor {
	public class TenorClient {
		private const string GIF_SEARCH_ENDPOINT = "https://g.tenor.com/v1/search";
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;
		private readonly JsonSerializerOptions _jsonSerializerOptions;

		public TenorClient(
			HttpClient httpClient,
			IOptions<TenorOptions> tenorOptionsAccessor
		) {
			TenorOptions tenorOptions = tenorOptionsAccessor.Value;
			_apiKey = tenorOptions.ApiKey ?? throw new InvalidOperationException("Tenor api key not configured. Please add a .NET secret with key 'TenorOptions:ApiKey' or a Docker secret with key 'TenorOptions__ApiKey'");
			_httpClient = httpClient;
			_jsonSerializerOptions = new JsonSerializerOptions {
				PropertyNamingPolicy = new SnakeCaseNamingPolicy()
			};
		}

		public async Task<(string Id, string Url, string PreviewUrl)[]> SearchGifsAsync(string query, CancellationToken cancellationToken) {
			string requestUrl = $"{GIF_SEARCH_ENDPOINT}?key={_apiKey}&q={WebUtility.UrlEncode(query)}&locale=id_ID&contentfilter=high&media_filter=minimal";
			GifSearchResult? gifSearchResult = await _httpClient.GetFromJsonAsync<GifSearchResult>(requestUrl, _jsonSerializerOptions, cancellationToken);
			return gifSearchResult?.Results.Select(result => (
				Id: result.Id,
				Url: result.Media[0][MediaFormatType.Gif].Url,
				PreviewUrl: result.Media[0][MediaFormatType.TinyGif].Preview
			)).ToArray() ?? Array.Empty<(string Id, string Url, string PreviewUrl)>();
		}
	}
}
