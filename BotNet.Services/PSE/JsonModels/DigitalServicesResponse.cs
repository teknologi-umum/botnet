using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace BotNet.Services.PSE.JsonModels {
	public record DigitalServicesResponse(
		[property: JsonPropertyName("meta")] DigitalServicesResponseMetadata Metadata,
		[property: JsonPropertyName("data")] ImmutableList<DigitalService> DigitalServices
	);
}
