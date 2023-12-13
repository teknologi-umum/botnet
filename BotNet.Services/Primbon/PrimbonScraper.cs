using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace BotNet.Services.Primbon {
	public class PrimbonScraper(HttpClient httpClient) {
		private const string KAMAROKAM_URL = "https://www.primbon.com/petung_hari_baik.php";
		private readonly HttpClient _httpClient = httpClient;

		public async Task<(string Title, string[] Traits)> GetKamarokamAsync(DateOnly date, CancellationToken cancellationToken) {
			using HttpRequestMessage httpRequest = new(HttpMethod.Post, KAMAROKAM_URL);
			using StringContent tgl = new(date.ToString("d"));
			using StringContent bln = new(date.ToString("M"));
			using StringContent thn = new(date.ToString("yyyy"));
			using StringContent submit = new("Submit!");
			httpRequest.Content = new MultipartFormDataContent {
				{ tgl, "tgl" },
				{ bln, "bln" },
				{ thn, "thn" },
				{ submit, "submit" }
			};
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
	}
}
