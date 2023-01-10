using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotNet.Services.BMKG {
	public class LatestEarthQuake : BMKG {
		public LatestEarthQuake(HttpClient client) : base(client) {}

		public async Task<(string Text, string ShakemapUrl)> GetLatestAsync() {
			string url = string.Format(uriTemplate, "autogempa");

			HttpResponseMessage response = await httpClient.GetAsync(url);
			response.EnsureSuccessStatusCode();

			if (response.Content.Headers.ContentType!.MediaType is not "application/json") {
				throw new HttpRequestException("Wrong Content Type");
			}

			Stream bodyContent = await response.Content.ReadAsStreamAsync();

			EarthQuake? bodyResponse = await JsonSerializer.DeserializeAsync<EarthQuake>(bodyContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			if (bodyResponse is null) {
				throw new JsonException("Failed to parse body");
			}

			string textResult = "<b>Gempa Terkini</b>\n"
				+ $"Magnitudo: {bodyResponse.InfoGempa.Gempa.Magnitude}\n"
				+ $"Tanggal: {bodyResponse.InfoGempa.Gempa.Tanggal} {bodyResponse.InfoGempa.Gempa.Jam}\n"
				+ $"Koordinat: {bodyResponse.InfoGempa.Gempa.Coordinates}\n"
				+ $"Kedalaman: {bodyResponse.InfoGempa.Gempa.Kedalaman}\n"
				+ $"Wilayah: {bodyResponse.InfoGempa.Gempa.Wilayah}\n"
				+ $"Potensi: {bodyResponse.InfoGempa.Gempa.Potensi}\n"
				+ "\n\nJaga diri, keluarga dan orang tersayang anda";

			string shakemapUrl = bodyResponse.InfoGempa.Gempa.ShakemapUrl;

			return (
				Text: textResult,
				ShakemapUrl: shakemapUrl
				);
		}
	}
}
