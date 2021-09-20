namespace BotNet.Services.OpenGraph.Models {
	public record OpenGraphMetadata(
		string? Title,
		string? Type,
		string? Image,
		string? ImageType,
		int? ImageWidth,
		int? ImageHeight,
		string? Description
	);
}
