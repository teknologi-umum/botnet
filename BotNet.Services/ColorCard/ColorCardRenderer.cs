using System;
using System.Globalization;
using System.IO;
using BotNet.Services.Typography;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;

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

			Color fillColor = Color.FromRgb(
				red: (byte)(colorValue / 65536),
				green: (byte)(colorValue / 256 % 256),
				blue: (byte)(colorValue % 256)
			);
			Color textColor = fillColor.GetLuminosity() < 0.5f
				? Color.FromRgba(0xff, 0xff, 0xff, 0xdd)
				: Color.FromRgba(0x00, 0x00, 0x00, 0xdd);

			BitmapExportContext exportContext = SkiaGraphicsService.Instance.CreateBitmapExportContext(200, 200, displayScale: 4f);

			ICanvas canvas = exportContext.Canvas;

			canvas.SaveState();
			canvas.FillColor = fillColor;
			canvas.FillRectangle(0f, 0f, 200f, 200f);
			canvas.RestoreState();

			Fonts.RegisterGlobalService(_botNetFontService);

			canvas.SaveState();
			canvas.FontName = "JetBrainsMonoNL-Regular";
			canvas.FontColor = textColor;
			canvas.FontSize = 24f;
			canvas.DrawString(trimmedColorName.ToUpperInvariant(), 0f, 0f, 200f, 200f, HorizontalAlignment.Center, VerticalAlignment.Center);
			canvas.RestoreState();

			using MemoryStream memoryStream = new();
			exportContext.WriteToStream(memoryStream);

			return memoryStream.ToArray();
		}
	}
}
