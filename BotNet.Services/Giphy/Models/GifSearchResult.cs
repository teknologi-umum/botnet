namespace BotNet.Services.Giphy.Models {
	/// <summary>
	/// https://developers.giphy.com/docs/api/endpoint#search
	/// </summary>
	public record GifSearchResult(
		GifObject[] Data
	);
}
