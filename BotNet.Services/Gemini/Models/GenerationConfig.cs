using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace BotNet.Services.Gemini.Models {
	public sealed record GenerationConfig(
		[property: JsonPropertyName("stopSequences"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ImmutableList<string>? StopSequences = null,
		[property: JsonPropertyName("temperature"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] double? Temperature = null,
		[property: JsonPropertyName("maxOutputTokens"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? MaxOutputTokens = null,
		[property: JsonPropertyName("topP"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] double? TopP = null,
		[property: JsonPropertyName("topK"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? TopK = null
	);
}
