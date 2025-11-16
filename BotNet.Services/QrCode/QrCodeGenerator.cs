using QRCoder;
using SkiaSharp;

namespace BotNet.Services.QrCode {
	public sealed class QrCodeGenerator {
		public byte[] GenerateQrCode(string url) {
			// Generate QR code data
			using QRCodeGenerator qrGenerator = new();
			using QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M);
			
			// Convert to bitmap data
			int pixelsPerModule = 20;
			int quietZoneModules = 4;
			int moduleCount = qrCodeData.ModuleMatrix.Count;
			int imageSize = (moduleCount + quietZoneModules * 2) * pixelsPerModule;

			using SKBitmap bitmap = new(imageSize, imageSize);
			using SKCanvas canvas = new(bitmap);

			// White background
			canvas.Clear(SKColors.White);

			using SKPaint paint = new() {
				Color = SKColors.Black,
				IsAntialias = false,
				Style = SKPaintStyle.Fill
			};

			// Draw QR code modules
			int offset = quietZoneModules * pixelsPerModule;
			for (int row = 0; row < moduleCount; row++) {
				for (int col = 0; col < moduleCount; col++) {
					if (qrCodeData.ModuleMatrix[row][col]) {
						SKRect rect = new(
							left: offset + col * pixelsPerModule,
							top: offset + row * pixelsPerModule,
							right: offset + (col + 1) * pixelsPerModule,
							bottom: offset + (row + 1) * pixelsPerModule
						);
						canvas.DrawRect(rect, paint);
					}
				}
			}

			// Encode to PNG
			using SKImage image = SKImage.FromBitmap(bitmap);
			using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
			return data.ToArray();
		}
	}
}
