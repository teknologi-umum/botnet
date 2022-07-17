using System.IO;
using SkiaSharp;

namespace BotNet.Services.Webp {
	public class WebpToImageConverter {
		public static byte[] Convert(byte[] originalImage) {
			SKBitmap bitmap = SKBitmap.Decode(originalImage);
			using SKSurface surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height));
			using SKCanvas canvas = surface.Canvas;

			canvas.DrawBitmap(
				bitmap: bitmap,
				source: SKRect.Create(bitmap.Width, bitmap.Height),
				dest: SKRect.Create(bitmap.Width, bitmap.Height));
			canvas.Flush();

			SKImage image = surface.Snapshot();
			SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

			using MemoryStream imageStream = new();
			data.SaveTo(imageStream);

			return imageStream.ToArray();
		}
	}
}
