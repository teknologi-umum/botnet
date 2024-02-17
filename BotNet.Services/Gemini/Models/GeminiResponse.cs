using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace BotNet.Services.Gemini.Models {
	public sealed record GeminiResponse(
		[property: JsonPropertyName("candidates")] ImmutableList<Candidate>? Candidates,
		[property: JsonPropertyName("promptFeedback")] PromptFeedback? PromptFeedback
	);
}
