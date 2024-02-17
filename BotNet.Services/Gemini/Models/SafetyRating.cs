using System.Text.Json.Serialization;

namespace BotNet.Services.Gemini.Models {
	public sealed record SafetyRating(
		[property: JsonPropertyName("category")] string Category,
		[property: JsonPropertyName("probability")] string Probability
	);
}
