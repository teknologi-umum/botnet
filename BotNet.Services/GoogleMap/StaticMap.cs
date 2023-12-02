using System;
using BotNet.Services.GoogleMap.Models;
using Microsoft.Extensions.Options;

namespace BotNet.Services.GoogleMap {

	/// <summary>
	/// Get static map image from google map api
	/// </summary>
	public class StaticMap {
		private readonly string? _apiKey;
		protected string mapPosition = "center";
		protected int zoom = 13;
		protected string size = "600x300";
		protected string marker = "color:red";
		private string _uriTemplate = "https://maps.googleapis.com/maps/api/staticmap";

		public StaticMap(IOptions<GoogleMapOptions> options) {
			_apiKey = options.Value.ApiKey;
		}

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

			Uri uri = new(_uriTemplate + $"?{mapPosition}={place}&zoom={zoom}&size={size}&markers={marker}|{place}&key={_apiKey}");

			return uri.ToString();
		}
	}
}
