using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace BotNet.Services.TrustPositif.JsonModels {
	public record BlacklistLookupResponse(
		[property: JsonPropertyName("response")] int Response,
		[property: JsonPropertyName("values")] ImmutableArray<BlacklistLookupResult> Values
	);
}
