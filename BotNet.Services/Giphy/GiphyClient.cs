using System.Net;
using System.Net.Http.Json;
using BotNet.Services.Giphy.Models;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Giphy {
	public class GiphyClient {
		private const string GIF_SEARCH_ENDPOINT = "https://api.giphy.com/v1/gifs/search";
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;

		public GiphyClient(
			HttpClient httpClient,
			IOptions<GiphyOptions> giphyOptionsAccessor
		) {
			GiphyOptions giphyOptions = giphyOptionsAccessor.Value;
			_apiKey = giphyOptions.ApiKey ?? throw new InvalidOperationException("Giphy api key not configured.");
			_httpClient = httpClient;
		}

		public async Task<GifObject[]> SearchGifsAsync(string query, CancellationToken cancellationToken) {
			string requestUrl = $"{GIF_SEARCH_ENDPOINT}?api_key={_apiKey}&q={WebUtility.UrlEncode(query)}&offset=0&limit=10&rating=g&lang=id";
			GifSearchResult? searchResult = await _httpClient.GetFromJsonAsync<GifSearchResult>(requestUrl, cancellationToken);
			return searchResult?.Data ?? Array.Empty<GifObject>();
		}
	}
}
