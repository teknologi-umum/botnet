using System;
using Microsoft.Extensions.Options;

namespace BotNet.Services.GoogleMap {

	/// <summary>
	/// Get static map image from google map api
	/// </summary>
	public class StaticMap(
		IOptions<GoogleMapOptions> options
	) {
		private readonly string? _apiKey = options.Value.ApiKey;
		private const string MapPosition = "center";
		private const int Zoom = 13;
		private const string Size = "600x300";
		private const string Marker = "color:red";
		private const string UriTemplate = "https://maps.googleapis.com/maps/api/staticmap";

		/// <summary>
		/// Get static map image from google map api
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

			Uri uri = new($"{UriTemplate}?{MapPosition}={place}&zoom={Zoom}&size={Size}&markers={Marker}|{place}&key={_apiKey}");

			return uri.ToString();
		}
	}
}
