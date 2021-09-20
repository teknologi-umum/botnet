namespace BotNet.Services.DuckDuckGo.Models {
	public record SearchResultItem(
		string Url,
		string Title,
		string UrlText,
		string IconUrl,
		string Snippet
	);
}
