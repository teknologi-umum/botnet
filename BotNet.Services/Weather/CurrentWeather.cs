using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BotNet.Services.Weather.Models;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Weather {
	public class CurrentWeather : Weather {
		public CurrentWeather(
			IOptions<WeatherOptions> options,
			HttpClient httpClient
			) : base(options, httpClient) {

		}

		public async Task<(string Text, string Icon)> GetCurrentWeatherAsync(string? place) {
			string url = string.Format(_uriTemplate, "current");
			if (string.IsNullOrEmpty(place)) {
				throw new ArgumentNullException(nameof(place));
			}

			if (string.IsNullOrEmpty(_apiKey)) {
				throw new ArgumentNullException(nameof(_apiKey));
			}

			Uri uri = new(url + $"?key={_apiKey}&q={place}");
			HttpResponseMessage response = await _httpClient.GetAsync(uri.AbsoluteUri);

			if (response is not { StatusCode: HttpStatusCode.OK, Content.Headers.ContentType.MediaType: string contentType }) {
				throw new HttpRequestException("Unable to find location.");
			}

			if (response.Content is not object && contentType is not "application/json") {
				throw new HttpRequestException("Failed to parse result.");
			}

			Stream bodyContent = await response.Content!.ReadAsStreamAsync();

			CurrentWeatherResponse? weatherResponse = await JsonSerializer.DeserializeAsync<CurrentWeatherResponse>(bodyContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
			if (bodyContent is null) {
				throw new HttpRequestException("Failed to parse result.");
			}

			string textResult = $"<b>Cuaca {place} saat ini</b>\n"
					 + $"Local Time: {weatherResponse!.Location!.LocalTime}\n"
					 + $"Condition: {weatherResponse!.Current!.Condition!.Text}\n"
					 + $"Temperature: {weatherResponse!.Current!.Temp_C} Celcius\n"
					 + $"Wind: {weatherResponse!.Current!.Wind_Kph} Kph";

			string icon = weatherResponse!.Current!.Condition!.Icon!;

			return (
				Text: textResult,
				Icon: icon.Remove(0, 2)
				);
		}
	}
}
