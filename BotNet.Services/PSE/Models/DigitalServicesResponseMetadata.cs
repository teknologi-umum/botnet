using System.Text.Json.Serialization;

namespace BotNet.Services.PSE.Models {
	public record DigitalServicesResponseMetadata(
		[property: JsonPropertyName("page")] PaginationMetadata PaginationMetadata
	);
}
