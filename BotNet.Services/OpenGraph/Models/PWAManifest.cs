using System.Text.Json.Serialization;

namespace BotNet.Services.OpenGraph.Models {
	public record PWAManifest(
		string? Name,
		string? ShortName,
		string? Description,
		string? StartUrl,
		string? Orientation,
		string? Display,
		string? BackgroundColor,
		string? ThemeColor,
		PWAIcon[]? Icons,
		[property: JsonPropertyName("lang")] string? Language,
		string? Status
	);
}
