using System.Text.Json.Serialization;

namespace BotNet.Services.Gemini.Models {
	public sealed record Part(
		[property: JsonPropertyName("text"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Text = null,
		[property: JsonPropertyName("inline_data"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] InlineData? InlineData = null
	);
}
