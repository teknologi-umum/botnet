using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace BotNet.Services.Gemini.Models {
	public sealed record Candidate(
		[property: JsonPropertyName("content")] Content? Content,
		[property: JsonPropertyName("finishReason")] string? FinishReason,
		[property: JsonPropertyName("index")] int? Index,
		[property: JsonPropertyName("safetyRatings")] ImmutableList<SafetyRating>? SafetyRatings
	);
}
