using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using BotNet.Services.OpenGraph.Models;

namespace BotNet.Services.OpenGraph {
	public class OpenGraphService {
		private readonly HttpClient _httpClient;

		public OpenGraphService(
			HttpClient httpClient
		) {
			_httpClient = httpClient;
		}

		public async Task<OpenGraphMetadata> GetMetadataAsync(string url, CancellationToken cancellationToken) {
			using HttpRequestMessage httpRequest = new(HttpMethod.Get, url) {
				Headers = {
					{ "Accept", "text/html" },
					{ "User-Agent", "TEKNUM" }
				}
			};
			using HttpResponseMessage response = await _httpClient.SendAsync(httpRequest, cancellationToken);
			response.EnsureSuccessStatusCode();

			string html = await response.Content.ReadAsStringAsync(cancellationToken);

			IBrowsingContext browsingContext = BrowsingContext.New(Configuration.Default);
			IDocument document = await browsingContext.OpenAsync(req => req.Content(html), cancellationToken);

			return new OpenGraphMetadata(
				Title: document.QuerySelector("meta[property='og:title']")?.GetAttribute("content"),
				Type: document.QuerySelector("meta[property='og:type']")?.GetAttribute("content"),
				Image: document.QuerySelector("meta[property='og:image']")?.GetAttribute("content"),
				ImageType: document.QuerySelector("meta[property='og:image:type']")?.GetAttribute("content"),
				ImageWidth: document.QuerySelector("meta[property='og:image:width']")?.GetAttribute("content") is string imageWidthString && int.TryParse(imageWidthString, out int imageWidth) ? imageWidth : null,
				ImageHeight: document.QuerySelector("meta[property='og:image:height']")?.GetAttribute("content") is string imageHeightString && int.TryParse(imageHeightString, out int imageHeight) ? imageHeight : null,
				Description: document.QuerySelector("meta[property='og:description']")?.GetAttribute("content")
			);
		}
	}
}
