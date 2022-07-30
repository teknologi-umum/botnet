using Newtonsoft.Json;

namespace BotNet.Services.PSE.Models {
	public record TimestampsResponse(
		[property: JsonProperty("data")] Timestamps Timestamps
	);
}
