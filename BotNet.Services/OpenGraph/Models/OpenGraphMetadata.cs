using Orleans;

namespace BotNet.Services.OpenGraph.Models {
	[GenerateSerializer]
	public record OpenGraphMetadata {
		[Id(0)]
		public string? Title { get; set; }

		[Id(1)]
		public string? Type { get; set; }

		[Id(2)]
		public string? Image { get; set; }

		[Id(3)]
		public string? ImageType { get; set; }

		[Id(4)]
		public int? ImageWidth { get; set; }

		[Id(5)]
		public int? ImageHeight { get; set; }

		[Id(6)]
		public string? Description { get; set; }
	}
}
