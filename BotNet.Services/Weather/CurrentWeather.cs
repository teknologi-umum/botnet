using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Weather.Models;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Weather {
	public class CurrentWeather(
		IOptions<WeatherOptions> options,
		HttpClient httpClient
	) : Weather(options, httpClient) {
		public async Task<(string Text, string Icon)> GetCurrentWeatherAsync(
			string? place,
			CancellationToken cancellationToken
		) {
			string url = string.Format(UriTemplate, "current");
			if (string.IsNullOrEmpty(place)) {
				throw new ArgumentNullException(nameof(place));
			}

			if (string.IsNullOrEmpty(ApiKey)) {
				throw new ArgumentNullException(nameof(ApiKey));
			}

			Uri uri = new($"{url}?key={ApiKey}&q={place}");
			HttpResponseMessage response = await HttpClient.GetAsync(uri.AbsoluteUri, cancellationToken);

			if (response is not { StatusCode: HttpStatusCode.OK, Content.Headers.ContentType.MediaType: string contentType }) {
				throw new HttpRequestException("Unable to find location.");
			}

			if (response.Content is not object &&
			    contentType is not "application/json") {
				throw new HttpRequestException("Failed to parse result.");
			}

			Stream bodyContent = await response.Content!.ReadAsStreamAsync(cancellationToken);

			CurrentWeatherResponse? weatherResponse = await JsonSerializer.DeserializeAsync<CurrentWeatherResponse>(bodyContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);

			if (weatherResponse is null) {
				throw new JsonException("Failed to parse result.");
			}

			string textResult = $"""
			                     <b>Cuaca {place} saat ini</b>
			                     Local Time: {weatherResponse.Location!.LocalTime}
			                     Condition: {weatherResponse.Current!.Condition!.Text}
			                     Temperature: {weatherResponse.Current!.Temp_C} Celcius
			                     Wind: {weatherResponse.Current!.Wind_Kph} km/h
			                     """;

			string icon = weatherResponse.Current!.Condition!.Icon!;

			return (
				Text: textResult,
				Icon: icon.Remove(0, 2)
			);
		}
	}
}
