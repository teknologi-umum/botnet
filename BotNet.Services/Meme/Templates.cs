using SkiaSharp;

namespace BotNet.Services.Meme {
	internal sealed record Template(
		string ImageResourceName,
		string FontStyleId,
		SKTextAlign TextAlign,
		SKColor TextColor,
		float TextSize,
		float LineHeight,
		float MaxWidth,
		float Rotation,
		float X,
		float Y
	);

	internal static class Templates {
		public static readonly Template Ramad = new(
			ImageResourceName: "BotNet.Services.Meme.Images.Ramad.jpg",
			FontStyleId: "Inter-Regular",
			TextAlign: SKTextAlign.Left,
			TextColor: new SKColor(0x00, 0x00, 0x00, 0xcc),
			TextSize: 17f,
			LineHeight: 20f,
			MaxWidth: 110f,
			Rotation: 1.4f,
			X: 120f,
			Y: 100f
		);
	}
}
