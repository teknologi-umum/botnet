using System;
using Microsoft.Extensions.Options;
using BotNet.Services.GoogleMap.Models;

namespace BotNet.Services.GoogleMap {

	/// <summary>
	/// Get static map image from google map api
	/// </summary>
	public class StaticMap(
		IOptions<GoogleMapOptions> options
	) {
		private readonly string? _apiKey = options.Value.ApiKey;
		private const string MapPosition = "center";
		private const string Size = "600x300";
		private const string Marker = "color:red";
		private const string UriTemplate = "https://maps.googleapis.com/maps/api/staticmap";

		/// <summary>
		/// Calculate appropriate zoom level based on viewport bounding box
		/// </summary>
		/// <param name="viewport">Viewport from geocoding result</param>
		/// <returns>Zoom level (1-20)</returns>
		public static int CalculateZoomLevel(Viewport? viewport) {
			if (viewport?.Northeast == null || viewport.Southwest == null) {
				return 13; // Default zoom
			}

			// Calculate the span of the viewport
			double latSpan = Math.Abs(viewport.Northeast.Lat - viewport.Southwest.Lat);
			double lngSpan = Math.Abs(viewport.Northeast.Lng - viewport.Southwest.Lng);
			double maxSpan = Math.Max(latSpan, lngSpan);

			// Calculate zoom level based on span
			// Zoom levels: 1 (world) to 20 (buildings)
			// Each zoom level roughly doubles the detail
			if (maxSpan >= 180) return 1;  // World
			if (maxSpan >= 90) return 2;
			if (maxSpan >= 45) return 3;
			if (maxSpan >= 22.5) return 4;
			if (maxSpan >= 11.25) return 5; // Large region
			if (maxSpan >= 5.625) return 6;
			if (maxSpan >= 2.813) return 7;
			if (maxSpan >= 1.406) return 8;
			if (maxSpan >= 0.703) return 9;
			if (maxSpan >= 0.352) return 10; // City
			if (maxSpan >= 0.176) return 11;
			if (maxSpan >= 0.088) return 12;
			if (maxSpan >= 0.044) return 13; // District
			if (maxSpan >= 0.022) return 14;
			if (maxSpan >= 0.011) return 15; // Streets
			if (maxSpan >= 0.005) return 16;
			if (maxSpan >= 0.0025) return 17;
			if (maxSpan >= 0.00125) return 18;
			if (maxSpan >= 0.000625) return 19;
			return 20; // Buildings
		}

		/// <summary>
		/// Get static map image URL with custom zoom level
		/// </summary>
		/// <param name="lat">Latitude</param>
		/// <param name="lng">Longitude</param>
		/// <param name="zoom">Zoom level (1-20)</param>
		/// <param name="markerLabel">Optional marker label</param>
		/// <returns>string of url</returns>
		public string GetMapUrl(double lat, double lng, int zoom, string? markerLabel = null) {
			if (string.IsNullOrEmpty(_apiKey)) {
				return "Api key is needed";
			}

			string marker = markerLabel != null 
				? $"{Marker}|label:{markerLabel}|{lat},{lng}"
				: $"{Marker}|{lat},{lng}";

			Uri uri = new($"{UriTemplate}?{MapPosition}={lat},{lng}&zoom={zoom}&size={Size}&markers={marker}&key={_apiKey}");

			return uri.ToString();
		}

		/// <summary>
		/// Get static map image from google map api (legacy method)
		/// </summary>
		/// <param name="place">Place or address that you want to search</param>
		/// <returns>string of url</returns>
		public string SearchPlace(string? place) {
			if (string.IsNullOrEmpty(place)) {
				return "Invalid place";
			}

			if (string.IsNullOrEmpty(_apiKey)) {
				return "Api key is needed";
			}

			Uri uri = new($"{UriTemplate}?{MapPosition}={place}&zoom=13&size={Size}&markers={Marker}|{place}&key={_apiKey}");

			return uri.ToString();
		}
	}
}
