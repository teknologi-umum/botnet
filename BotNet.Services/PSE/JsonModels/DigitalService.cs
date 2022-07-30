using System.Text.Json.Serialization;

namespace BotNet.Services.PSE.JsonModels {
	public record DigitalService(
		[property: JsonPropertyName("id")] int Id,
		[property: JsonPropertyName("type")] string Type,
		[property: JsonPropertyName("attributes")] DigitalServiceAttributes Attributes
	);
}
