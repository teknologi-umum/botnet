using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace BotNet.Services.PSE.Models {
	public record DigitalServicesResponse(
		[property: JsonPropertyName("meta")] DigitalServicesResponseMetadata Metadata,
		[property: JsonPropertyName("data")] ImmutableList<DigitalService> DigitalServices
	);
}
