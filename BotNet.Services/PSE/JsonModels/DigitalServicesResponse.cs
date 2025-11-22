using System.Text.Json.Serialization;

namespace BotNet.Services.PSE.JsonModels {
	public record DigitalServicesResponse(
		[property: JsonPropertyName("data")] DigitalServicesData Data,
		[property: JsonPropertyName("error")] string Error,
		[property: JsonPropertyName("message")] string Message,
		[property: JsonPropertyName("status")] string Status,
		[property: JsonPropertyName("status_code")] int StatusCode
	);
}
