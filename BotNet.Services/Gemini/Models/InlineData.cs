using System.Text.Json.Serialization;

namespace BotNet.Services.Gemini.Models {
	public sealed record InlineData(
		[property: JsonPropertyName("mime_type")] string MimeType,
		[property: JsonPropertyName("data")] string Data
	);
}
