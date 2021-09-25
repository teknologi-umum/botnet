using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using BotNet.Services.BotSanitizers;
using BotNet.Services.Json;
using BotNet.Services.StackExchange.Models;

namespace BotNet.Services.StackExchange {
	public class StackExchangeClient {
		private const string SEARCH_ENDPOINT = "https://api.stackexchange.com/2.3/search";
		private readonly HttpClient _httpClient;
		private readonly JsonSerializerOptions _jsonSerializerOptions;

		public StackExchangeClient(
			HttpClient httpClient
		) {
			_httpClient = httpClient;
			_jsonSerializerOptions = new JsonSerializerOptions {
				PropertyNamingPolicy = new SnakeCaseNamingPolicy()
			};
		}

		public async Task<ImmutableList<StackExchangeQuestionSnippet>> SearchStackOverflowAsync(string tag, string intitle, CancellationToken cancellationToken) {
			string url = $"{SEARCH_ENDPOINT}?sort=relevance&order=desc{(string.IsNullOrWhiteSpace(tag) ? "" : $"&tagged={WebUtility.UrlEncode(tag)}")}&intitle={WebUtility.UrlEncode(intitle)}&site=stackoverflow";
			using HttpRequestMessage request = new(HttpMethod.Get, url);
			using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			StackExchangeSearchResult? searchResult;
			using Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
			if (response.Content.Headers.ContentEncoding.Contains("gzip")) {
				using GZipStream gzipStream = new(contentStream, CompressionMode.Decompress);
				searchResult = await JsonSerializer.DeserializeAsync<StackExchangeSearchResult>(gzipStream, _jsonSerializerOptions, cancellationToken);
			} else {
				searchResult = await JsonSerializer.DeserializeAsync<StackExchangeSearchResult>(contentStream, _jsonSerializerOptions, cancellationToken);
			}

			return searchResult?.Items ?? ImmutableList<StackExchangeQuestionSnippet>.Empty;
		}

		public async Task<(string? Question, string? AcceptedAnswer)> GetQuestionAndAcceptedAnswerAsync(string url, CancellationToken cancellationToken) {
			using HttpRequestMessage request = new(HttpMethod.Get, url);
			using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			string html = await response.Content.ReadAsStringAsync(cancellationToken);

			IBrowsingContext browsingContext = BrowsingContext.New(Configuration.Default);
			IDocument document = await browsingContext.OpenAsync(req => req.Content(html), cancellationToken);

			return (
				Question: document.QuerySelector(".question .postcell > [itemprop='text']")?.InnerHtml,
				AcceptedAnswer: document.QuerySelector(".accepted-answer .answercell > [itemprop='text']")?.InnerHtml
			);
		}

		public async Task<string?> TryAskAsync(string query, CancellationToken cancellationToken) {
			ImmutableList<StackExchangeQuestionSnippet> searchResult = await SearchStackOverflowAsync("", query, cancellationToken);
			if (searchResult.FirstOrDefault(result => result.IsAnswered) is StackExchangeQuestionSnippet firstResult
				&& await GetQuestionAndAcceptedAnswerAsync(firstResult.Link, cancellationToken) is (string question, string answerHtml)) {
				return $"<b><a href=\"{firstResult.Link}\">{firstResult.Title}</a></b>\n\n<b><u>QUESTION:</u></b>\n\n{TextMessageSanitizer.SanitizeHtml(question)}\n\n\n<b><u>ACCEPTED ANSWER:</u></b>\n\n{TextMessageSanitizer.SanitizeHtml(answerHtml)}";
			} else {
				return null;
			}
		}
	}
}
