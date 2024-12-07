using System;
using System.Net;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Options;
using BotNet.Services.GoogleMap.Models;
using System.IO;
using System.Threading.Tasks;

namespace BotNet.Services.GoogleMap {

	/// <summary>
	/// This class intended to get geocoding from address.
	/// </summary>
	public class GeoCode(
		HttpClient httpClient,
		IOptions<GoogleMapOptions> options
	) {
		private readonly string? _apiKey = options.Value.ApiKey;
		private const string UriTemplate = "https://maps.googleapis.com/maps/api/geocode/json";

		/// <summary>
		/// The response of this api call is consist of 2 parts.
		/// Array of "results" and string of "status"
		/// 
		/// Even though the results is array, the docs say normally the result will have only one element.
		/// So, we can grab the result like result[0]
		/// </summary>
		/// <param name="place">Place or address that you want to search</param>
		/// <returns>strings of coordinates</returns>
		/// <exception cref="HttpRequestException"></exception>
		public async Task<(double Lat, double Lng)> SearchPlaceAsync(string? place) {
			if (string.IsNullOrEmpty(place)) {
				throw new HttpRequestException("Invalid place");
			}

			if (string.IsNullOrEmpty(_apiKey)) {
				throw new HttpRequestException("Api key is needed");
			}

			Uri uri = new($"{UriTemplate}?address={place}&key={_apiKey}");
			HttpResponseMessage response = await httpClient.GetAsync(uri.AbsoluteUri);

			if (response is not { StatusCode: HttpStatusCode.OK, Content.Headers.ContentType.MediaType: string contentType }) {
				throw new HttpRequestException("Unable to find location.");
			}

			if (response.Content is null && contentType is not "application/json") {
				throw new HttpRequestException("Failed to parse result.");
			}

			Stream bodyContent = await response.Content!.ReadAsStreamAsync();

			Response? body = await JsonSerializer.DeserializeAsync<Response>(bodyContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			if (body is null) {
				throw new HttpRequestException("Failed to parse result.");
			}

			if (body.Status is not "OK") {
				throw new HttpRequestException("Unable to find location.");
			}

			if (body.Results!.Count <= 0) {
				throw new HttpRequestException("No Result.");
			}

			Result result = body.Results[0];

			double lat = result.Geometry!.Location!.Lat;
			double lng = result.Geometry!.Location!.Lng;

			return (lat, lng);
		}
	}
}
