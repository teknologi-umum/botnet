using System;
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

		public static bool IsShortenedTiktokLink(Uri link) {
			return Regex.IsMatch(link.OriginalString, "^https://vt.tiktok.com/[0-9a-zA-Z]{8,12}/$");
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
