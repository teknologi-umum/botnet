using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace BotNet.Services.Primbon {
	public class PrimbonScraper(
		HttpClient httpClient
	) {
		private const string KAMAROKAM_URL = "https://www.primbon.com/petung_hari_baik.php";
		private const string TALIWANGKE_URL = "https://primbon.com/hari_sangar_taliwangke.php";
		private readonly HttpClient _httpClient = httpClient;

		public async Task<(string Title, string[] Traits)> GetKamarokamAsync(DateOnly date, CancellationToken cancellationToken) {
			using HttpRequestMessage httpRequest = new(HttpMethod.Post, KAMAROKAM_URL);
			using FormUrlEncodedContent content = new(
				nameValueCollection: new List<KeyValuePair<string, string>>() {
					new("tgl", date.Day.ToString()),
					new("bln", date.Month.ToString()),
					new("thn", date.Year.ToString()),
					new("submit", " Submit! ")
				}
			);
			httpRequest.Content = content;
			using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
			httpResponse.EnsureSuccessStatusCode();

			string html = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

			IBrowsingContext browsingContext = BrowsingContext.New(Configuration.Default);
			IDocument document = await browsingContext.OpenAsync(req => req.Content(html), cancellationToken);
			IHtmlElement? iElement = document.QuerySelector<IHtmlElement>("div#body > i");
			IHtmlElement? bElement = document.QuerySelector<IHtmlElement>("div#body > i > b");

			string? title = bElement?.InnerHtml;
			string? traits = iElement?.InnerHtml;

			if (title is null || traits is null) {
				throw new InvalidOperationException("Primbon.com returned an unexpected response.");
			}

			if (traits.IndexOf("</b>") is int index and not -1) {
				traits = traits[(index + 4)..];
			}

			return (
				Title: title.Trim(),
				Traits: traits.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			);
		}

		public async Task<(string JavaneseDate, string Title, string Description)> GetTaliwangkeAsync(DateOnly date, CancellationToken cancellationToken) {
			using HttpRequestMessage httpRequest = new(HttpMethod.Post, TALIWANGKE_URL);
			using FormUrlEncodedContent content = new(
				nameValueCollection: new List<KeyValuePair<string, string>>() {
					new("tgl", date.Day.ToString()),
					new("bln", date.Month.ToString()),
					new("thn", date.Year.ToString()),
					new("kirim", " Submit! ")
				}
			);
			httpRequest.Content = content;
			using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
			httpResponse.EnsureSuccessStatusCode();

			string html = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

			IBrowsingContext browsingContext = BrowsingContext.New(Configuration.Default);
			IDocument document = await browsingContext.OpenAsync(req => req.Content(html), cancellationToken);
			IHtmlElement? bodyDiv = document.QuerySelector<IHtmlElement>("div#body");
			IHtmlElement? iElement = document.QuerySelector<IHtmlElement>("div#body > i");
			IHtmlElement? bElement = document.QuerySelector<IHtmlElement>("div#body > i > b");

			string? body = bodyDiv?.InnerHtml;
			string? title = bElement?.InnerHtml;
			string? desc = iElement?.InnerHtml;

			if (body is null || title is null || desc is null) {
				throw new InvalidOperationException("Primbon.com returned an unexpected response.");
			}

			if (body.IndexOf("<br>") is int index1 and not -1) {
				body = body[(index1 + 4)..];
				if (body.IndexOf("<br>") is int index2 and not -1) {
					body = body[..index2];
				}
			}

			if (desc.IndexOf("</b>") is int index3 and not -1) {
				desc = desc[(index3 + 4)..];
			}

			return (
				JavaneseDate: body.Trim(),
				Title: title.Trim(),
				Description: desc.Trim()
			);
		}
	}
}
