using System.Text.Json.Serialization;

namespace BotNet.Services.Tenor.Models {
	public record MediaObject(
		string Preview,
		string Url,
		string Size,
		[property: JsonPropertyName("dims")] int[] Dimensions
	);
}
