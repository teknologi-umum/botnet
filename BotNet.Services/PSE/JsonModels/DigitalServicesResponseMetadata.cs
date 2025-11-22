using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace BotNet.Services.PSE.JsonModels {
	public sealed record DigitalServicesData(
		[property: JsonPropertyName("data")] ImmutableList<DigitalService> Data,
		[property: JsonPropertyName("total_rows")] int TotalRows
	);
}
