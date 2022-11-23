using System;
using System.Globalization;
using System.IO;
using BotNet.Services.Typography;
using SkiaSharp;

namespace BotNet.Services.ColorCard {
	public class ColorCardRenderer {
		private readonly BotNetFontService _botNetFontService;

		public ColorCardRenderer(
			BotNetFontService botNetFontService
		) {
			_botNetFontService = botNetFontService;
		}

		public byte[] RenderColorCard(string colorName) {
			if (string.IsNullOrWhiteSpace(colorName)) throw new ArgumentNullException(nameof(colorName));

			string trimmedColorName = colorName.Trim();

			if (trimmedColorName.Length is not 4 and not 7) throw new ArgumentException("Color name must be 3-digit or 6-digit hexadecimal string.", nameof(colorName));
			if (trimmedColorName[0] != '#') throw new ArgumentException("Color name must be 3-digit or 6-digit hexadecimal string.", nameof(colorName));

			// Convert #rgb to #rrggbb
			string normalizedName = trimmedColorName.Length == 4
				? $"#{trimmedColorName[1]}{trimmedColorName[1]}{trimmedColorName[2]}{trimmedColorName[2]}{trimmedColorName[3]}{trimmedColorName[3]}"
				: trimmedColorName;

			if (!int.TryParse(normalizedName[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int colorValue)) throw new ArgumentException("Color name must be 3-digit or 6-digit hexadecimal string.", nameof(colorName));

			SKColor fillColor = new(
				(byte)((colorValue >> 16) & 0xFF),
				(byte)((colorValue >> 8) & 0xFF),
				(byte)(colorValue & 0xFF)
			);
			fillColor.ToHsl(out _, out _, out float luminosity);
			SKColor textColor = luminosity < 50f
				? new SKColor(0xff, 0xff, 0xff, 0xdd)
				: new SKColor(0x00, 0x00, 0x00, 0xdd);

			using SKBitmap bitmap = new(400, 400);
			using SKSurface surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height));
			using SKCanvas canvas = surface.Canvas;

			canvas.Clear(fillColor);

			using Stream fontStream = _botNetFontService.GetFontStyleById("JetBrainsMonoNL-Regular").OpenStream();
			using SKTypeface typeface = SKTypeface.FromStream(fontStream);
			using SKPaint paint = new() {
				TextAlign = SKTextAlign.Center,
				Color = textColor,
				Typeface = typeface,
				TextSize = 50f,
				IsAntialias = true
			};
			SKRect textBound = new();
			paint.MeasureText(normalizedName, ref textBound);
			canvas.DrawText(
				text: trimmedColorName.ToUpperInvariant(),
				x: 200f,
				y: 200f - textBound.Height / 2f,
				paint: paint
			);

			using SKImage image = surface.Snapshot();
			using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

			using MemoryStream memoryStream = new();
			data.SaveTo(memoryStream);

			return memoryStream.ToArray();
		}
	}
}
