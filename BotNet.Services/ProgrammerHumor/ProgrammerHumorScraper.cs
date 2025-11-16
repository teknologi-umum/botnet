using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace BotNet.Services.ProgrammerHumor {
	public class ProgrammerHumorScraper(HttpClient httpClient) {
		public async Task<(string Title, byte[] Image)> GetRandomJokeAsync(CancellationToken cancellationToken) {
			const string url = "https://programmerhumor.io/?bimber_random_post=true";
			using HttpRequestMessage httpRequest = new(HttpMethod.Get, url);
			using HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
			httpResponse.EnsureSuccessStatusCode();

			string html = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

			IBrowsingContext browsingContext = BrowsingContext.New(Configuration.Default);
			IDocument document = await browsingContext.OpenAsync(req => req.Content(html), cancellationToken);
			IHtmlHeadingElement? titleElement = document.QuerySelector<IHtmlHeadingElement>(".post .post-header h2.post-title");
			IHtmlImageElement? imageElement = document.QuerySelector<IHtmlImageElement>(".post .post-image img");

			string? src = imageElement?.Dataset["src"] ?? imageElement?.Source;
			if (string.IsNullOrWhiteSpace(src)) {
				throw new InvalidOperationException("Could not find image source in the HTML response");
			}

			return (
				Title: titleElement?.InnerHtml ?? "Humor",
				Image: await httpClient.GetByteArrayAsync(src, cancellationToken)
			);
		}
	}
}
