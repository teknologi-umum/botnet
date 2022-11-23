using System.IO;
using SkiaSharp;

namespace BotNet.Services.ImageFlip {
	public class Flipper {
		public static byte[] FlipImage(byte[] originalImage) {
			using SKBitmap bitmap = SKBitmap.Decode(originalImage);
			using SKSurface surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height));
			using SKCanvas canvas = surface.Canvas;

			canvas.Save();
			canvas.DrawBitmap(
				bitmap: bitmap,
				source: SKRect.Create(bitmap.Width / 2f, bitmap.Height),
				dest: SKRect.Create(bitmap.Width / 2f, bitmap.Height));
			canvas.Scale(-1, 1, bitmap.Width / 2f, 0);
			canvas.DrawBitmap(
				bitmap: bitmap,
				source: SKRect.Create(bitmap.Width / 2f, bitmap.Height),
				dest: SKRect.Create(bitmap.Width / 2f, bitmap.Height));
			canvas.Restore();
			canvas.Flush();

			using SKImage image = surface.Snapshot();
			using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

			using MemoryStream flippedImageStream = new();
			data.SaveTo(flippedImageStream);

			return flippedImageStream.ToArray();
		}

		public static byte[] FlopImage(byte[] originalImage) {
			using SKBitmap bitmap = SKBitmap.Decode(originalImage);
			using SKSurface surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height));
			using SKCanvas canvas = surface.Canvas;

			canvas.Save();
			canvas.DrawBitmap(
				bitmap: bitmap,
				source: SKRect.Create(bitmap.Width / 2f, 0f, bitmap.Width / 2f, bitmap.Height),
				dest: SKRect.Create(bitmap.Width / 2f, 0f, bitmap.Width / 2f, bitmap.Height));
			canvas.Scale(-1, 1, bitmap.Width / 2f, 0);
			canvas.DrawBitmap(
				bitmap: bitmap,
				source: SKRect.Create(bitmap.Width / 2f, 0f, bitmap.Width / 2f, bitmap.Height),
				dest: SKRect.Create(bitmap.Width / 2f, 0f, bitmap.Width / 2f, bitmap.Height));
			canvas.Restore();
			canvas.Flush();

			using SKImage image = surface.Snapshot();
			using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

			using MemoryStream flippedImageStream = new();
			data.SaveTo(flippedImageStream);

			return flippedImageStream.ToArray();
		}
	}
}
