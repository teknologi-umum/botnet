using System;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using BotNet.Services.DuckDuckGo.Models;

namespace BotNet.Services.DuckDuckGo {
	public class DuckDuckGoClient {
		private const string HTML_SEARCH_ENDPOINT = "https://html.duckduckgo.com/html/";
		private const string PROXY_LINK_PREFIX = "http://duckduckgo.com/l/?uddg=";
		private const string PROXY_LINK_DELIMITER = "&rut=";
		private readonly HttpClient _httpClient;

		public DuckDuckGoClient(
			HttpClient httpClient
		) {
			_httpClient = httpClient;
		}

		public async Task<ImmutableList<SearchResultItem>> SearchAsync(string query, CancellationToken cancellationToken) {
			string url = $"{HTML_SEARCH_ENDPOINT}?kp=1&q={WebUtility.UrlEncode(query)}";
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
			IHtmlCollection<IElement> resultItemNodes = document.QuerySelectorAll(".web-result");

			ImmutableList<SearchResultItem>.Builder resultItemsBuilder = ImmutableList.CreateBuilder<SearchResultItem>();

			foreach (IElement resultItemNode in resultItemNodes) {
				if (resultItemNode.QuerySelector<IHtmlAnchorElement>(".result__url") is { Href: string itemUrl }
					&& resultItemNode.QuerySelector<IHtmlAnchorElement>(".result__title > a") is { TextContent: string itemTitle }
					&& resultItemNode.QuerySelector<IHtmlAnchorElement>(".result__url") is { TextContent: string itemUrlText } && itemUrlText.Trim() is string trimmedItemUrlText
					&& resultItemNode.QuerySelector<IHtmlImageElement>(".result__icon__img") is { Source: string itemIconUrl }
					&& resultItemNode.QuerySelector<IHtmlAnchorElement>(".result__snippet") is { TextContent: string itemSnippet }) {
					if (itemUrl.StartsWith(PROXY_LINK_PREFIX, StringComparison.InvariantCultureIgnoreCase)) {
						itemUrl = itemUrl[PROXY_LINK_PREFIX.Length..];
						int delimiterIndex = itemUrl.IndexOf(PROXY_LINK_DELIMITER, StringComparison.InvariantCultureIgnoreCase);
						if (delimiterIndex != -1) itemUrl = itemUrl[..delimiterIndex];
						itemUrl = WebUtility.UrlDecode(itemUrl);
					}
					resultItemsBuilder.Add(new SearchResultItem(
						Url: itemUrl,
						Title: itemTitle,
						UrlText: trimmedItemUrlText,
						IconUrl: itemIconUrl,
						Snippet: itemSnippet
					));
				}
			}

			return resultItemsBuilder.ToImmutable();
		}
	}
}
