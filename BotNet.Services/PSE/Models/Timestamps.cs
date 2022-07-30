using System;
using System.Text.Json.Serialization;

namespace BotNet.Services.PSE.Models {
	public record Timestamps(
		[property: JsonPropertyName("generated_at")] DateTime LastGenerated
	);
}
