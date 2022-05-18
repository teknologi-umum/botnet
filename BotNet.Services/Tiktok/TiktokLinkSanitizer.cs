using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.Tiktok {
	public class TiktokLinkSanitizer : IDisposable {
		private readonly HttpClientHandler _httpClientHandler;
		private readonly HttpClient _httpClient;
		private bool _disposedValue;

		public TiktokLinkSanitizer() {
			_httpClientHandler = new() {
				AllowAutoRedirect = false
			};
			_httpClient = new(_httpClientHandler);
		}

		public async Task<Uri> SanitizeAsync(Uri link, CancellationToken cancellationToken) {
			using HttpResponseMessage response = await _httpClient.GetAsync(link, cancellationToken);
			if (response is not { StatusCode: HttpStatusCode.MovedPermanently, Headers.Location: Uri redirectedLink }) {
				throw new HttpRequestException("Link was not redirected");
			}
			string sanitizedUri = redirectedLink.GetLeftPart(UriPartial.Path);
			return new Uri(sanitizedUri);
		}

		public static Uri? FindShortenedTiktokLink(string message) {
			return Regex.Matches(message, "https://vt.tiktok.com/[0-9a-zA-Z]{8,12}/")
				.Select(match => new Uri(match.Value))
				.FirstOrDefault();
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
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
