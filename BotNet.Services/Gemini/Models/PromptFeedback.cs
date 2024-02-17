using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace BotNet.Services.Gemini.Models {
	public sealed record PromptFeedback(
		[property: JsonPropertyName("safetyRatings")] ImmutableList<SafetyRating> SafetyRatings
	);
}
