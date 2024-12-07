using System.Net.Http;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Weather {
	public class Weather(
		IOptions<WeatherOptions> options,
		HttpClient httpClient
	) {
		protected readonly string? ApiKey = options.Value.ApiKey;
		protected const string UriTemplate = "https://api.weatherapi.com/v1/{0}.json";
		protected readonly HttpClient HttpClient = httpClient;
	}
}
