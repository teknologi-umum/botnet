using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Weather.Models;

namespace BotNet.Services.Weather {
	/// <summary>
	/// Client for wttr.in weather service
	/// </summary>
	public sealed class WttrInWeather(
		HttpClient httpClient
	) {
		private const string BaseUrl = "https://wttr.in";

		/// <summary>
		/// Get weather information for a location
		/// </summary>
		/// <param name="location">Location name, can be city name, coordinates, airport code, etc.</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Weather response from wttr.in</returns>
		public async Task<WttrInResponse?> GetWeatherAsync(
			string location,
			CancellationToken cancellationToken
		) {
			if (string.IsNullOrWhiteSpace(location)) {
				throw new ArgumentException("Location cannot be empty", nameof(location));
			}

			// URL encode the location
			string encodedLocation = Uri.EscapeDataString(location);
			
			// Use ?format=j1 for JSON output
			// Use ?m for metric units
			string url = $"{BaseUrl}/{encodedLocation}?format=j1&m";

			HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
			response.EnsureSuccessStatusCode();

			string json = await response.Content.ReadAsStringAsync(cancellationToken);
			
			WttrInResponse? weatherResponse = JsonSerializer.Deserialize<WttrInResponse>(
				json,
				new JsonSerializerOptions {
					PropertyNameCaseInsensitive = true
				}
			);

			return weatherResponse;
		}

		/// <summary>
		/// Format weather information for display
		/// </summary>
		public static string FormatWeatherReport(WttrInResponse response, string searchedLocation) {
			if (response.current_condition == null || response.current_condition.Length == 0) {
				throw new InvalidOperationException("No current weather data available");
			}

			if (response.weather == null || response.weather.Length < 3) {
				throw new InvalidOperationException("Not enough forecast data available");
			}

			CurrentCondition current = response.current_condition[0];
			
			// Get location information
			string locationName = searchedLocation;
			string coordinates = "";
			if (response.nearest_area != null && response.nearest_area.Length > 0) {
				NearestArea area = response.nearest_area[0];
				string? areaName = area.areaName?[0]?.value;
				string? regionName = area.region?[0]?.value;
				string? countryName = area.country?[0]?.value;
				
				if (!string.IsNullOrEmpty(areaName)) {
					locationName = areaName;
					if (!string.IsNullOrEmpty(regionName) && regionName != areaName) {
						locationName += $", {regionName}";
					}
					if (!string.IsNullOrEmpty(countryName)) {
						locationName += $", {countryName}";
					}
				}
				
				if (!string.IsNullOrEmpty(area.latitude) && !string.IsNullOrEmpty(area.longitude)) {
					coordinates = $" [{area.latitude},{area.longitude}]";
				}
			}

			System.Text.StringBuilder report = new();
			
			// Location
			report.AppendLine($"📍 <b>Location: {locationName}{coordinates}</b>");
			report.AppendLine();
			
			// Current weather
			string weatherCondition = current.weatherDesc?[0]?.value ?? "Unknown";
			string weatherEmoji = GetWeatherEmoji(current.weatherCode);
			
			report.AppendLine($"<b>Current Weather</b>");
			report.AppendLine($"{weatherEmoji} {weatherCondition}");
			report.AppendLine($"🌡️ {current.temp_C}°C (feels like {current.FeelsLikeC}°C)");
			report.AppendLine($"{GetWindDirectionArrow(current.winddir16Point)} Wind: {current.windspeedKmph} km/h");
			report.AppendLine($"💧 Humidity: {current.humidity}%");
			report.AppendLine($"👁️ Visibility: {current.visibility} km");
			
			string currentPrecipMM = current.precipMM ?? "0.0";
			report.AppendLine($"🌧️ Precipitation: {currentPrecipMM} mm");
			
			report.AppendLine();
			
			// Today (Day 0)
			if (response.weather.Length > 0) {
				WeatherForecast today = response.weather[0];
				report.AppendLine($"<b>{today.date}</b>");
				AppendDayForecast(report, today);
				report.AppendLine();
			}
			
			// Tomorrow (Day 1)
			if (response.weather.Length > 1) {
				WeatherForecast tomorrow = response.weather[1];
				report.AppendLine($"<b>{tomorrow.date}</b>");
				AppendDayForecast(report, tomorrow);
				report.AppendLine();
			}
			
			// Day after tomorrow (Day 2)
			if (response.weather.Length > 2) {
				WeatherForecast dayAfterTomorrow = response.weather[2];
				report.AppendLine($"<b>{dayAfterTomorrow.date}</b>");
				AppendDayForecast(report, dayAfterTomorrow);
				report.AppendLine();
			}
			
			report.AppendLine($"<i>Powered by wttr.in</i>");

			return report.ToString();
		}

		/// <summary>
		/// Append forecast for a specific day (morning, noon, evening, night)
		/// </summary>
		private static void AppendDayForecast(System.Text.StringBuilder report, WeatherForecast day) {
			if (day.hourly == null || day.hourly.Length < 8) {
				report.AppendLine($"🌡️ {day.mintempC}°C - {day.maxtempC}°C");
				return;
			}

			// wttr.in provides hourly data in 3-hour intervals (0, 3, 6, 9, 12, 15, 18, 21)
			// Morning: 6-9 (index 2-3), Noon: 12-15 (index 4-5), Evening: 18 (index 6), Night: 21 (index 7)
			
			// Morning (6 AM - 9 AM) - use 6 AM data
			if (day.hourly.Length > 2) {
				HourlyForecast morning = day.hourly[2];
				string morningEmoji = GetWeatherEmoji(morning.weatherCode);
				string windArrow = GetWindDirectionArrow(morning.winddir16Point);
				string precipMM = morning.precipMM ?? "0.0";
				string chanceOfRain = morning.chanceofrain ?? "0";
				report.AppendLine($"Morning: {morningEmoji} {morning.tempC}°C ({morning.FeelsLikeC}°C), {windArrow} {morning.windspeedKmph} km/h, 🌧️ {precipMM} mm ({chanceOfRain}%)");
			}
			
			// Noon (12 PM - 3 PM) - use 12 PM data
			if (day.hourly.Length > 4) {
				HourlyForecast noon = day.hourly[4];
				string noonEmoji = GetWeatherEmoji(noon.weatherCode);
				string windArrow = GetWindDirectionArrow(noon.winddir16Point);
				string precipMM = noon.precipMM ?? "0.0";
				string chanceOfRain = noon.chanceofrain ?? "0";
				report.AppendLine($"Noon: {noonEmoji} {noon.tempC}°C ({noon.FeelsLikeC}°C), {windArrow} {noon.windspeedKmph} km/h, 🌧️ {precipMM} mm ({chanceOfRain}%)");
			}
			
			// Evening (6 PM) - use 6 PM data
			if (day.hourly.Length > 6) {
				HourlyForecast evening = day.hourly[6];
				string eveningEmoji = GetWeatherEmoji(evening.weatherCode);
				string windArrow = GetWindDirectionArrow(evening.winddir16Point);
				string precipMM = evening.precipMM ?? "0.0";
				string chanceOfRain = evening.chanceofrain ?? "0";
				report.AppendLine($"Evening: {eveningEmoji} {evening.tempC}°C ({evening.FeelsLikeC}°C), {windArrow} {evening.windspeedKmph} km/h, 🌧️ {precipMM} mm ({chanceOfRain}%)");
			}
			
			// Night (9 PM - midnight) - use 9 PM data
			if (day.hourly.Length > 7) {
				HourlyForecast night = day.hourly[7];
				string? moonPhase = day.astronomy?[0]?.moon_phase;
				string nightEmoji = GetWeatherEmoji(night.weatherCode, isNight: true, moonPhase: moonPhase);
				string windArrow = GetWindDirectionArrow(night.winddir16Point);
				string precipMM = night.precipMM ?? "0.0";
				string chanceOfRain = night.chanceofrain ?? "0";
				report.AppendLine($"Night: {nightEmoji} {night.tempC}°C ({night.FeelsLikeC}°C), {windArrow} {night.windspeedKmph} km/h, 🌧️ {precipMM} mm ({chanceOfRain}%)");
			}
		}

		/// <summary>
		/// Get emoji for weather condition code
		/// </summary>
		/// <param name="weatherCode">Weather condition code</param>
		/// <param name="isNight">Whether it's nighttime (default: false)</param>
		/// <param name="moonPhase">Moon phase for clear nights (optional)</param>
		private static string GetWeatherEmoji(string? weatherCode, bool isNight = false, string? moonPhase = null) {
			// Night-specific emojis for clear/partly cloudy conditions
			if (isNight) {
				return weatherCode switch {
					"113" => !string.IsNullOrEmpty(moonPhase) ? GetMoonPhaseEmoji(moonPhase) : "🌙",  // Clear night - use moon phase
					"116" => "☁️",  // Partly cloudy night (could use 🌙 with cloud but keeping consistent)
					"119" => "☁️",  // Cloudy
					"122" => "☁️",  // Overcast
					"143" => "🌫️",  // Mist
					"176" => "🌦️",  // Patchy rain possible
					"179" => "🌨️",  // Patchy snow possible
					"182" => "🌧️",  // Patchy sleet possible
					"185" => "🌧️",  // Patchy freezing drizzle possible
					"200" => "⛈️",  // Thundery outbreaks possible
					"227" => "🌨️",  // Blowing snow
					"230" => "🌨️",  // Blizzard
					"248" => "🌫️",  // Fog
					"260" => "🌫️",  // Freezing fog
					"263" => "🌧️",  // Patchy light drizzle
					"266" => "🌧️",  // Light drizzle
					"281" => "🌧️",  // Freezing drizzle
					"284" => "🌧️",  // Heavy freezing drizzle
					"293" => "🌦️",  // Patchy light rain
					"296" => "🌧️",  // Light rain
					"299" => "🌧️",  // Moderate rain at times
					"302" => "🌧️",  // Moderate rain
					"305" => "🌧️",  // Heavy rain at times
					"308" => "🌧️",  // Heavy rain
					"311" => "🌧️",  // Light freezing rain
					"314" => "🌧️",  // Moderate or heavy freezing rain
					"317" => "🌨️",  // Light sleet
					"320" => "🌨️",  // Moderate or heavy sleet
					"323" => "🌨️",  // Patchy light snow
					"326" => "🌨️",  // Light snow
					"329" => "🌨️",  // Patchy moderate snow
					"332" => "🌨️",  // Moderate snow
					"335" => "🌨️",  // Patchy heavy snow
					"338" => "🌨️",  // Heavy snow
					"350" => "🌨️",  // Ice pellets
					"353" => "🌦️",  // Light rain shower
					"356" => "🌧️",  // Moderate or heavy rain shower
					"359" => "🌧️",  // Torrential rain shower
					"362" => "🌨️",  // Light sleet showers
					"365" => "🌨️",  // Moderate or heavy sleet showers
					"368" => "🌨️",  // Light snow showers
					"371" => "🌨️",  // Moderate or heavy snow showers
					"374" => "🌨️",  // Light showers of ice pellets
					"377" => "🌨️",  // Moderate or heavy showers of ice pellets
					"386" => "⛈️",  // Patchy light rain with thunder
					"389" => "⛈️",  // Moderate or heavy rain with thunder
					"392" => "⛈️",  // Patchy light snow with thunder
					"395" => "⛈️",  // Moderate or heavy snow with thunder
					_ => "🌡️"      // Default
				};
			}
			
			// Day emojis
			return weatherCode switch {
				"113" => "☀️",  // Sunny
				"116" => "🌤️",  // Partly cloudy
				"119" => "☁️",  // Cloudy
				"122" => "☁️",  // Overcast
				"143" => "🌫️",  // Mist
				"176" => "🌦️",  // Patchy rain possible
				"179" => "🌨️",  // Patchy snow possible
				"182" => "🌧️",  // Patchy sleet possible
				"185" => "🌧️",  // Patchy freezing drizzle possible
				"200" => "⛈️",  // Thundery outbreaks possible
				"227" => "🌨️",  // Blowing snow
				"230" => "🌨️",  // Blizzard
				"248" => "🌫️",  // Fog
				"260" => "🌫️",  // Freezing fog
				"263" => "🌧️",  // Patchy light drizzle
				"266" => "🌧️",  // Light drizzle
				"281" => "🌧️",  // Freezing drizzle
				"284" => "🌧️",  // Heavy freezing drizzle
				"293" => "🌦️",  // Patchy light rain
				"296" => "🌧️",  // Light rain
				"299" => "🌧️",  // Moderate rain at times
				"302" => "🌧️",  // Moderate rain
				"305" => "🌧️",  // Heavy rain at times
				"308" => "🌧️",  // Heavy rain
				"311" => "🌧️",  // Light freezing rain
				"314" => "🌧️",  // Moderate or heavy freezing rain
				"317" => "🌨️",  // Light sleet
				"320" => "🌨️",  // Moderate or heavy sleet
				"323" => "🌨️",  // Patchy light snow
				"326" => "🌨️",  // Light snow
				"329" => "🌨️",  // Patchy moderate snow
				"332" => "🌨️",  // Moderate snow
				"335" => "🌨️",  // Patchy heavy snow
				"338" => "🌨️",  // Heavy snow
				"350" => "🌨️",  // Ice pellets
				"353" => "🌦️",  // Light rain shower
				"356" => "🌧️",  // Moderate or heavy rain shower
				"359" => "🌧️",  // Torrential rain shower
				"362" => "🌨️",  // Light sleet showers
				"365" => "🌨️",  // Moderate or heavy sleet showers
				"368" => "🌨️",  // Light snow showers
				"371" => "🌨️",  // Moderate or heavy snow showers
				"374" => "🌨️",  // Light showers of ice pellets
				"377" => "🌨️",  // Moderate or heavy showers of ice pellets
				"386" => "⛈️",  // Patchy light rain with thunder
				"389" => "⛈️",  // Moderate or heavy rain with thunder
				"392" => "⛈️",  // Patchy light snow with thunder
				"395" => "⛈️",  // Moderate or heavy snow with thunder
				_ => "🌡️"      // Default
			};
		}

		/// <summary>
		/// Get emoji for moon phase
		/// </summary>
		private static string GetMoonPhaseEmoji(string moonPhase) {
			return moonPhase.ToLowerInvariant() switch {
				string s when s.Contains("new moon") => "🌑",
				string s when s.Contains("waxing crescent") => "🌒",
				string s when s.Contains("first quarter") => "🌓",
				string s when s.Contains("waxing gibbous") => "🌔",
				string s when s.Contains("full moon") => "🌕",
				string s when s.Contains("waning gibbous") => "🌖",
				string s when s.Contains("last quarter") => "🌗",
				string s when s.Contains("waning crescent") => "🌘",
				_ => "🌙"
			};
		}

		/// <summary>
		/// Get directional arrow emoji for wind direction
		/// </summary>
		private static string GetWindDirectionArrow(string? windDirection) {
			return windDirection?.ToUpperInvariant() switch {
				"N" => "⬇️",      // North wind blows down (from north to south)
				"NNE" => "⬇️",
				"NE" => "↙️",
				"ENE" => "↙️",
				"E" => "⬅️",
				"ESE" => "↖️",
				"SE" => "↖️",
				"SSE" => "⬆️",
				"S" => "⬆️",      // South wind blows up (from south to north)
				"SSW" => "⬆️",
				"SW" => "↗️",
				"WSW" => "↗️",
				"W" => "➡️",
				"WNW" => "↘️",
				"NW" => "↘️",
				"NNW" => "⬇️",
				_ => "💨"
			};
		}
	}
}
