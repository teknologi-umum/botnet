using System.Text.Json.Serialization;

namespace BotNet.Services.PSE.Models {
	public record TimestampsResponse(
		[property: JsonPropertyName("data")] Timestamps Timestamps
	);
}
