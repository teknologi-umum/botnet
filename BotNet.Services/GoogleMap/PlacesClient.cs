using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.GoogleMap.Models;
using Microsoft.Extensions.Options;

namespace BotNet.Services.GoogleMap {
	/// <summary>
	/// Client for Google Places API
	/// </summary>
	public sealed class PlacesClient(
		HttpClient httpClient,
		IOptions<GoogleMapOptions> googleMapOptions
	) {
		private readonly GoogleMapOptions _googleMapOptions = googleMapOptions.Value;

		/// <summary>
		/// Get detailed information about a place using its place_id
		/// </summary>
		/// <param name="placeId">The place_id from Geocoding API</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Place details or null if not found</returns>
		public async Task<PlaceDetails?> GetPlaceDetailsAsync(
			string placeId,
			CancellationToken cancellationToken
		) {
			// Specify fields to retrieve - grouped by billing category
			// Basic: name, formatted_address, geometry, business_status, url
			// Contact: formatted_phone_number, international_phone_number, website, opening_hours
			// Atmosphere: rating, user_ratings_total, price_level, editorial_summary
			string fields = string.Join(",",
				"name",
				"formatted_address",
				"business_status",
				"url",
				"formatted_phone_number",
				"international_phone_number",
				"website",
				"opening_hours",
				"rating",
				"user_ratings_total",
				"price_level",
				"editorial_summary"
			);

			string url = $"https://maps.googleapis.com/maps/api/place/details/json" +
				$"?place_id={Uri.EscapeDataString(placeId)}" +
				$"&fields={Uri.EscapeDataString(fields)}" +
				$"&key={_googleMapOptions.ApiKey}";

			HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
			response.EnsureSuccessStatusCode();

			string json = await response.Content.ReadAsStringAsync(cancellationToken);
			PlaceDetailsResponse? placeDetailsResponse = JsonSerializer.Deserialize<PlaceDetailsResponse>(
				json,
				new JsonSerializerOptions {
					PropertyNameCaseInsensitive = true
				}
			);

			if (placeDetailsResponse?.Status == "OK") {
				return placeDetailsResponse.Result;
			}

			return null;
		}
	}
}
