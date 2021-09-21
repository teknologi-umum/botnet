using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace BotNet.Services.ImageConverter {
	public class IcoToPngConverter {
		private readonly HttpClient _httpClient;

		public IcoToPngConverter(
			HttpClient httpClient
		) {
			_httpClient = httpClient;
		}

		public async Task<byte[]> ConvertFromUrlAsync(string url, CancellationToken cancellationToken) {
			using HttpRequestMessage httpRequest = new(HttpMethod.Get, url) {
				Headers = {
					{ "Accept", "image/ico" },
					{ "User-Agent", "TEKNUM" }
				}
			};
			using HttpResponseMessage response = await _httpClient.SendAsync(httpRequest, cancellationToken);
			response.EnsureSuccessStatusCode();

			using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);

			SKBitmap bitmap = SKBitmap.Decode(stream);
			SKImage image = SKImage.FromBitmap(bitmap);
			SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

			using MemoryStream memoryStream = new();
			data.SaveTo(memoryStream);

			return memoryStream.ToArray();
		}
	}
}
