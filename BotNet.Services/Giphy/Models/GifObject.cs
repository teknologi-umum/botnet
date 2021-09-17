namespace BotNet.Services.Giphy.Models {
	/// <summary>
	/// https://developers.giphy.com/docs/api/schema#gif-object
	/// </summary>
	public record GifObject(
		string Id,
		string Slug,
		string Url,
		string Rating, // y, g, pg, pg-13, r
		Images Images
	);
}
