using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace BotNet.Services.Gemini.Models {
	public sealed record GeminiRequest(
		[property: JsonPropertyName("contents")] ImmutableList<Content> Contents,
		[property: JsonPropertyName("safetySettings"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ImmutableList<SafetySettings>? SafetySettings,
		[property: JsonPropertyName("generationConfig"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] GenerationConfig? GenerationConfig
	);
}
