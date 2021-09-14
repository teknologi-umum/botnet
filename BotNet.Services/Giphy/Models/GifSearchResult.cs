using System.Text.Json.Serialization;

namespace BotNet.Services.Giphy.Models {
	/// <summary>
	/// https://developers.giphy.com/docs/api/endpoint#search
	/// </summary>
	public class GifSearchResult {
		[JsonPropertyName("data")]
		public GifObject[]? Data { get; set; }
	}
}
