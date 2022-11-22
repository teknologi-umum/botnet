using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using BotNet.Services.Hosting;
using BotNet.Services.Json;
using BotNet.Services.OpenGraph.Models;
using Microsoft.Extensions.Options;

namespace BotNet.Services.OpenGraph {
	public class OpenGraphService {
		private readonly HttpClient _httpClient;
		private readonly JsonSerializerOptions _jsonSerializerOptions;
		private readonly string? _hostName;

		public OpenGraphService(
			HttpClient httpClient,
			IOptions<HostingOptions> hostingOptionsAccessor
		) {
			_httpClient = httpClient;
			_jsonSerializerOptions = new JsonSerializerOptions {
				PropertyNamingPolicy = new SnakeCaseNamingPolicy()
			};
			_hostName = hostingOptionsAccessor.Value.HostName;
		}

		public async Task<OpenGraphMetadata> GetMetadataAsync(string url, CancellationToken cancellationToken) {
			Uri pageUri = new(url);
			using HttpRequestMessage request = new(HttpMethod.Get, pageUri) {
				Headers = {
					{ "Accept", "text/html" },
					{ "User-Agent", "TEKNUM" }
				}
			};
			using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			string html = await response.Content.ReadAsStringAsync(cancellationToken);

			IBrowsingContext browsingContext = BrowsingContext.New(Configuration.Default);
			IDocument document = await browsingContext.OpenAsync(req => req.Content(html), cancellationToken);

			// Get image from OpenGraph
			string? imageUrl = document.QuerySelector("meta[property='og:image']")?.GetAttribute("content");

			// Get image from manifest.json
			if (imageUrl == null) {
				string? manifestUrl = document.QuerySelector("link[rel='manifest']")?.GetAttribute("href");
				if (manifestUrl != null) {
					if (manifestUrl.StartsWith("/")) {
						manifestUrl = $"{pageUri.Scheme}://{pageUri.Host}{manifestUrl}";
					}
					PWAManifest? pwaManifest = await _httpClient.GetFromJsonAsync<PWAManifest>(manifestUrl, _jsonSerializerOptions, cancellationToken);
					if (pwaManifest?.Icons?.FirstOrDefault(icon => icon.Type is "image/png" or "image/jpeg" or "image/jpg" or "image/gif") is { Src: string iconSrc }) {
						imageUrl = iconSrc;
					}
				}
			}

			// Get image from favicon.ico
			if (imageUrl == null
				&& _hostName != null) {
				imageUrl = $"{pageUri.Scheme}://{pageUri.Host}/favicon.ico";
			}

			return new OpenGraphMetadata {
				Title = document.QuerySelector("meta[property='og:title']")?.GetAttribute("content"),
				Type = document.QuerySelector("meta[property='og:type']")?.GetAttribute("content"),
				Image = imageUrl,
				ImageType = document.QuerySelector("meta[property='og:image:type']")?.GetAttribute("content"),
				ImageWidth = document.QuerySelector("meta[property='og:image:width']")?.GetAttribute("content") is string imageWidthString && int.TryParse(imageWidthString, out int imageWidth) ? imageWidth : null,
				ImageHeight = document.QuerySelector("meta[property='og:image:height']")?.GetAttribute("content") is string imageHeightString && int.TryParse(imageHeightString, out int imageHeight) ? imageHeight : null,
				Description = document.QuerySelector("meta[property='og:description']")?.GetAttribute("content")
			};
		}
	}
}
