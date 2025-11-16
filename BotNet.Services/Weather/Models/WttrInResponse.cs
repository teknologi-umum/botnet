namespace BotNet.Services.Weather.Models {
	/// <summary>
	/// Response from wttr.in JSON API
	/// </summary>
	public class WttrInResponse {
		// ReSharper disable once InconsistentNaming
		public CurrentCondition[]? current_condition { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public WeatherForecast[]? weather { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public NearestArea[]? nearest_area { get; set; }
	}

	/// <summary>
	/// Current weather conditions
	/// </summary>
	public class CurrentCondition {
		// ReSharper disable once InconsistentNaming
		public string? temp_C { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? temp_F { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? FeelsLikeC { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? FeelsLikeF { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? humidity { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? cloudcover { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? windspeedKmph { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? windspeedMiles { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? winddirDegree { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? winddir16Point { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? precipMM { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? pressure { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? uvIndex { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? visibility { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? weatherCode { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public WeatherDesc[]? weatherDesc { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? observation_time { get; set; }
	}

	/// <summary>
	/// Weather forecast for a specific day
	/// </summary>
	public class WeatherForecast {
		public string? date { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? maxtempC { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? maxtempF { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? mintempC { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? mintempF { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? avgtempC { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? avgtempF { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? totalSnow_cm { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? sunHour { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? uvIndex { get; set; }
		
		public HourlyForecast[]? hourly { get; set; }
		
		public Astronomy[]? astronomy { get; set; }
	}

	/// <summary>
	/// Hourly weather forecast
	/// </summary>
	public class HourlyForecast {
		public string? time { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? tempC { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? tempF { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? FeelsLikeC { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? FeelsLikeF { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? windspeedKmph { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? winddir16Point { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? weatherCode { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public WeatherDesc[]? weatherDesc { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? precipMM { get; set; }
		
		public string? humidity { get; set; }
		
		public string? cloudcover { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? chanceofrain { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? chanceofsnow { get; set; }
	}

	/// <summary>
	/// Astronomical data for a specific day
	/// </summary>
	public class Astronomy {
		public string? sunrise { get; set; }
		public string? sunset { get; set; }
		public string? moonrise { get; set; }
		public string? moonset { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? moon_phase { get; set; }
		
		// ReSharper disable once InconsistentNaming
		public string? moon_illumination { get; set; }
	}

	/// <summary>
	/// Weather description
	/// </summary>
	public class WeatherDesc {
		public string? value { get; set; }
	}

	/// <summary>
	/// Nearest area information
	/// </summary>
	public class NearestArea {
		// ReSharper disable once InconsistentNaming
		public AreaName[]? areaName { get; set; }
		
		public Country[]? country { get; set; }
		
		public string? latitude { get; set; }
		public string? longitude { get; set; }
		
		public Region[]? region { get; set; }
		
		public string? population { get; set; }
	}

	/// <summary>
	/// Area name
	/// </summary>
	public class AreaName {
		public string? value { get; set; }
	}

	/// <summary>
	/// Country
	/// </summary>
	public class Country {
		public string? value { get; set; }
	}

	/// <summary>
	/// Region
	/// </summary>
	public class Region {
		public string? value { get; set; }
	}
}
