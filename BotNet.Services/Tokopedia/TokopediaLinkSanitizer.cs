using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.Tokopedia {
	public class TokopediaLinkSanitizer : IDisposable {
		private readonly HttpClientHandler _httpClientHandler;
		private readonly HttpClient _httpClient;
		private readonly string _userAgent;
		private bool _disposedValue;

		public TokopediaLinkSanitizer() {
			_httpClientHandler = new() {
				AllowAutoRedirect = false
			};
			_httpClient = new(_httpClientHandler);
			_userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36";
		}

		public static Uri? FindShortenedLink(string message) {
			return Regex.Matches(message, "https://tokopedia.(com|link)/[0-9a-zA-Z_]+")
				.Select(match => new Uri(match.Value))
				.FirstOrDefault();
		}


		public async Task<Uri> SanitizeAsync(Uri link, CancellationToken cancellationToken) {

			_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
			using HttpResponseMessage response = await _httpClient.GetAsync(link, cancellationToken);

			if (response is not { StatusCode: HttpStatusCode.TemporaryRedirect, Headers.Location: Uri firstStageRedirect }) {
				throw new HttpRequestException("Link was not redirected");
			}

			if (firstStageRedirect.OriginalString == "https://www.tokopedia.com") {
				throw new HttpRequestException("Invalid link");
			}

			Uri? secondLink = response.Headers.Location;
			using HttpResponseMessage secondResponse = await _httpClient.GetAsync(secondLink, cancellationToken);

			if (secondResponse is not { StatusCode: HttpStatusCode.TemporaryRedirect, Headers.Location: Uri secondStageRedirect}) {
				throw new HttpRequestException("Link was not redirected");
			}

			if (secondStageRedirect.OriginalString == "https://www.tokopedia.com") {
				throw new HttpRequestException("Invalid link");
			}

			Uri? redirectedLink = secondStageRedirect;
			string sanitizedUri = redirectedLink.GetLeftPart(UriPartial.Path);
			return new Uri(sanitizedUri);

		}

		protected virtual void Dispose(bool disposing) {
			if (!_disposedValue) {
				if (disposing) {
					// dispose managed state (managed objects)
					_httpClient.Dispose();
					_httpClientHandler.Dispose();
				}

				_disposedValue = true;
			}
		}

		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
