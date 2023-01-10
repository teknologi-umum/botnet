using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Weather {
	public class Weather {
		protected readonly string? _apiKey;
		protected string _uriTemplate = "https://api.weatherapi.com/v1/{0}.json";
		protected readonly HttpClient _httpClient;

		public Weather(IOptions<WeatherOptions> options, HttpClient httpClient) {
			_apiKey = options.Value.ApiKey;
			_httpClient = httpClient;
		}
	}
}
