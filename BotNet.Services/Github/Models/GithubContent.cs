using System.Text.Json.Serialization;

namespace BotNet.Services.Github.Models {
	public record GithubContent(
		[property: JsonConverter(typeof(JsonStringEnumConverter))] GithubContentType Type,
		string Encoding,
		long Size,
		string Name,
		string Path,
		[property: JsonPropertyName("content")] string EncodedContent,
		[property: JsonPropertyName("sha")] string SHA,
		string Url,
		string GitUrl,
		string HtmlUrl,
		string DownloadUrl,
		[property: JsonPropertyName("_links")] GithubLinks Links
	);
}
