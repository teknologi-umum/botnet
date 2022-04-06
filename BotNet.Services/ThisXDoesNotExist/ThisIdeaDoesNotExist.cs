using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace BotNet.Services.ThisXDoesNotExist {
	public class ThisIdeaDoesNotExist {
		private readonly HttpClient _httpClient;

		public ThisIdeaDoesNotExist(
			HttpClient httpClient
		) {
			_httpClient = httpClient;
		}

		public async Task<string> GetRandomIdeaAsync(CancellationToken cancellationToken) {
			const string url = "https://thisideadoesnotexist.com/";
			using HttpRequestMessage httpRequest = new(HttpMethod.Get, url);
			using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
			httpResponse.EnsureSuccessStatusCode();

			string html = await httpResponse.Content.ReadAsStringAsync();

			IBrowsingContext browsingContext = BrowsingContext.New(Configuration.Default);
			IDocument document = await browsingContext.OpenAsync(req => req.Content(html), cancellationToken);
			IHtmlHeadingElement titleElement = document.QuerySelector<IHtmlHeadingElement>("h2");

			return titleElement.InnerHtml;
		}
	}
}
