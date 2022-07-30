using System.Text.Json.Serialization;

namespace BotNet.Services.PSE.JsonModels {
	public record TimestampsResponse(
		[property: JsonPropertyName("data")] Timestamps Timestamps
	);
}
