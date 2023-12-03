using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BotNet.Services.OpenAI.Models {
	public record ImageGenerationResult(
		[property: JsonPropertyName("created")] int CreatedUnixTime,
		List<GeneratedImage> Data
	);

	public record GeneratedImage(
		string Url
	);
}
