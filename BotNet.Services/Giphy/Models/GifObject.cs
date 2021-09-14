using System.Text.Json.Serialization;

namespace BotNet.Services.Giphy.Models {
	/// <summary>
	/// https://developers.giphy.com/docs/api/schema#gif-object
	/// </summary>
	public class GifObject {
		[JsonPropertyName("id")]
		public string? Id { get; set; }

		[JsonPropertyName("slug")]
		public string? Slug { get; set; }

		[JsonPropertyName("url")]
		public string? Url { get; set; }

		/// <summary>
		/// y, g, pg, pg-13, r
		/// </summary>
		[JsonPropertyName("rating")]
		public string? Rating { get; set; }
	}
}
