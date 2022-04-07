using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace BotNet.Services.ProgrammerHumor {
	public class ProgrammerHumorScraper {
		private readonly HttpClient _httpClient;

		public ProgrammerHumorScraper(
			HttpClient httpClient
		) {
			_httpClient = httpClient;
		}

		public async Task<(string Title, byte[] Image)> GetRandomJokeAsync(CancellationToken cancellationToken) {
			const string url = "https://programmerhumor.io/?bimber_random_post=true";
			using HttpRequestMessage httpRequest = new(HttpMethod.Get, url);
			using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
			httpResponse.EnsureSuccessStatusCode();

			string html = await httpResponse.Content.ReadAsStringAsync();

			IBrowsingContext browsingContext = BrowsingContext.New(Configuration.Default);
			IDocument document = await browsingContext.OpenAsync(req => req.Content(html), cancellationToken);
			IHtmlHeadingElement titleElement = document.QuerySelector<IHtmlHeadingElement>("article header.entry-header h1.entry-title");
			IHtmlImageElement imageElement = document.QuerySelector<IHtmlImageElement>("article div[itemprop=\"image\"] img");

			return (
				Title: titleElement.InnerHtml,
				Image: await _httpClient.GetByteArrayAsync(imageElement.Source, cancellationToken)
			);
		}
	}
}
