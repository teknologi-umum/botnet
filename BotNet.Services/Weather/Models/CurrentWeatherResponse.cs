namespace BotNet.Services.Weather.Models {
	public class CurrentWeatherResponse {
		public Location? Location { get; set; }
		public Current? Current { get; set; }
	}
}
