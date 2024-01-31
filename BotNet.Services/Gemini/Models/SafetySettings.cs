using System.Text.Json.Serialization;

namespace BotNet.Services.Gemini.Models {
	public sealed record SafetySettings(
		[property: JsonPropertyName("category")] string Category,
		[property: JsonPropertyName("threshold")] string Threshold
	);
}
