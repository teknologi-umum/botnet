using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotNet.Services.BMKG {
	public class LatestEarthQuake(
		HttpClient client
	) : Bmkg(client) {
		private static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

		public async Task<(string Text, string ShakemapUrl)> GetLatestAsync() {
			string url = string.Format(UriTemplate, "autogempa");

			HttpResponseMessage response = await HttpClient.GetAsync(url);
			response.EnsureSuccessStatusCode();

			if (response.Content.Headers.ContentType!.MediaType is not "application/json") {
				throw new HttpRequestException("Wrong Content Type");
			}

			Stream bodyContent = await response.Content.ReadAsStreamAsync();

			EarthQuake? bodyResponse = await JsonSerializer.DeserializeAsync<EarthQuake>(bodyContent, JsonSerializerOptions);

			if (bodyResponse is null) {
				throw new JsonException("Failed to parse body");
			}

			string textResult = $"""
			                     <b>Gempa Terkini</b>
			                     Magnitudo: {bodyResponse.InfoGempa.Gempa.Magnitude}
			                     Tanggal: {bodyResponse.InfoGempa.Gempa.Tanggal} {bodyResponse.InfoGempa.Gempa.Jam}
			                     Koordinat: {bodyResponse.InfoGempa.Gempa.Coordinates}
			                     Kedalaman: {bodyResponse.InfoGempa.Gempa.Kedalaman}
			                     Wilayah: {bodyResponse.InfoGempa.Gempa.Wilayah}
			                     Potensi: {bodyResponse.InfoGempa.Gempa.Potensi}


			                     Jaga diri, keluarga dan orang tersayang anda
			                     """;

			string shakemapUrl = bodyResponse.InfoGempa.Gempa.ShakemapUrl;

			return (
				Text: textResult,
				ShakemapUrl: shakemapUrl
			);
		}
	}
}
