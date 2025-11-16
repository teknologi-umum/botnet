using System;
using System.Net;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Options;
using BotNet.Services.GoogleMap.Models;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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
		
		// Monas coordinates: 6°10′31.4″S 106°49′37.7″E = -6.175389, 106.827139
		private const double MonasLatitude = -6.175389;
		private const double MonasLongitude = 106.827139;

		// Bounding box around Jakarta (roughly 50km radius)
		// Southwest: -6.37, 106.65
		// Northeast: -5.98, 107.00
		private const string JakartaBounds = "-6.37,106.65|-5.98,107.00";

		// Bounding box around Jabodetabek (Jakarta metropolitan area)
		// Southwest: -6.73, 106.48 (includes Bogor, Depok, Tangerang, Bekasi)
		// Northeast: -5.87, 107.18
		private const string JabodetabekBounds = "-6.73,106.48|-5.87,107.18";

		// Bounding box around Java and Bali
		// Southwest: -8.78, 105.15 (Southern Java)
		// Northeast: -5.50, 116.35 (Eastern Bali)
		private const string JavaBaliBounds = "-8.78,105.15|-5.50,116.35";

		// Bounding box around Indonesia
		// Southwest: -11.00, 95.00 (Southern Java/Bali)
		// Northeast: 6.00, 141.00 (Northern Papua)
		private const string IndonesiaBounds = "-11.00,95.00|6.00,141.00";

		/// <summary>
		/// Search for places and return all results sorted by distance from Jakarta (Monas)
		/// Uses fallback strategy: Jakarta -> Jabodetabek -> Java+Bali -> Indonesia -> Worldwide
		/// </summary>
		/// <param name="place">Place or address that you want to search</param>
		/// <returns>List of results sorted by distance from Jakarta</returns>
		/// <exception cref="HttpRequestException"></exception>
		public async Task<List<Result>> SearchPlacesAsync(string? place) {
			if (string.IsNullOrEmpty(place)) {
				throw new HttpRequestException("Invalid place");
			}

			if (string.IsNullOrEmpty(_apiKey)) {
				throw new HttpRequestException("Api key is needed");
			}

			// Try Jakarta bounds first
			List<Result>? results = await SearchWithBoundsAsync(place, JakartaBounds);
			if (results != null && results.Count > 0) {
				return results;
			}

			// Fallback to Jabodetabek (Greater Jakarta)
			results = await SearchWithBoundsAsync(place, JabodetabekBounds);
			if (results != null && results.Count > 0) {
				return results;
			}

			// Fallback to Java + Bali
			results = await SearchWithBoundsAsync(place, JavaBaliBounds);
			if (results != null && results.Count > 0) {
				return results;
			}

			// Fallback to Indonesia bounds
			results = await SearchWithBoundsAsync(place, IndonesiaBounds);
			if (results != null && results.Count > 0) {
				return results;
			}

			// Fallback to worldwide search
			results = await SearchWithBoundsAsync(place, null);
			if (results != null && results.Count > 0) {
				return results;
			}

			throw new HttpRequestException("No Result.");
		}

		/// <summary>
		/// Search with optional bounds parameter
		/// </summary>
		/// <param name="place">Place or address to search</param>
		/// <param name="bounds">Optional bounds parameter (southwest|northeast)</param>
		/// <returns>List of results sorted by distance from Jakarta, or null if search failed</returns>
		private async Task<List<Result>?> SearchWithBoundsAsync(string place, string? bounds) {
			string url = string.IsNullOrEmpty(bounds)
				? $"{UriTemplate}?address={place}&key={_apiKey}"
				: $"{UriTemplate}?address={place}&bounds={bounds}&key={_apiKey}";

			Uri uri = new(url);
			HttpResponseMessage response = await httpClient.GetAsync(uri.AbsoluteUri);

			if (response is not { StatusCode: HttpStatusCode.OK, Content.Headers.ContentType.MediaType: string contentType }) {
				return null;
			}

			if (response.Content is null || contentType is not "application/json") {
				return null;
			}

			Stream bodyContent = await response.Content!.ReadAsStreamAsync();

			Response? body = await JsonSerializer.DeserializeAsync<Response>(bodyContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			if (body is null || body.Status is not "OK" || body.Results is null || body.Results.Count == 0) {
				return null;
			}

			// Sort results by distance from Monas (Jakarta center)
			List<Result> sortedResults = body.Results
				.Where(r => r.Geometry?.Location != null)
				.OrderBy(r => CalculateDistance(
					MonasLatitude, 
					MonasLongitude, 
					r.Geometry!.Location!.Lat, 
					r.Geometry!.Location!.Lng
				))
				.ToList();

			return sortedResults;
		}

		/// <summary>
		/// Calculate distance between two coordinates using Haversine formula
		/// </summary>
		/// <returns>Distance in kilometers</returns>
		private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2) {
			const double R = 6371; // Earth's radius in kilometers
			double dLat = ToRadians(lat2 - lat1);
			double dLon = ToRadians(lon2 - lon1);
			
			double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
					   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
					   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
			
			double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
			return R * c;
		}

		private static double ToRadians(double degrees) {
			return degrees * Math.PI / 180.0;
		}

		/// <summary>
		/// Legacy method for backward compatibility
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
			List<Result> results = await SearchPlacesAsync(place);
			Result result = results[0];
			
			double lat = result.Geometry!.Location!.Lat;
			double lng = result.Geometry!.Location!.Lng;

			return (lat, lng);
		}
	}
}
