using System.Text.Json.Serialization;

namespace BotNet.Services.PSE.JsonModels {
	public record DigitalServicesResponseMetadata(
		[property: JsonPropertyName("page")] PaginationMetadata PaginationMetadata
	);
}
