using System;
using System.Collections.Generic;
using System.IO;
using BotNet.Services.Typography;
using SkiaSharp;

namespace BotNet.Services.Meme {
	public class MemeGenerator(
		BotNetFontService botNetFontService
	) {
		public byte[] CaptionRamad(string text) {
			return CaptionMeme(Templates.Ramad, text);
		}

		private byte[] CaptionMeme(Template template, string text) {
			using Stream templateStream = typeof(MemeGenerator).Assembly.GetManifestResourceStream(template.ImageResourceName)!;
			using SKBitmap templateBitmap = SKBitmap.Decode(templateStream);
			using SKSurface surface = SKSurface.Create(new SKImageInfo(templateBitmap.Width, templateBitmap.Height));
			using SKCanvas canvas = surface.Canvas;

			SKRect templateRect = SKRect.Create(templateBitmap.Width, templateBitmap.Height);
			canvas.DrawBitmap(
				bitmap: templateBitmap,
				source: templateRect,
				dest: templateRect
			);

			canvas.Save();
			canvas.RotateDegrees(template.Rotation);
			using Stream fontStream = botNetFontService.GetFontStyleById(template.FontStyleId).OpenStream();
			using SKTypeface typeface = SKTypeface.FromStream(fontStream);
			using SKPaint paint = new();
			paint.TextAlign = template.TextAlign;
			paint.Color = template.TextColor;
			paint.Typeface = typeface;
			paint.TextSize = template.TextSize;
			paint.IsAntialias = true;
			float offset = 0f;
			foreach (string line in WrapWords(text, template.MaxWidth, paint)) {
				canvas.DrawText(
					text: line,
					x: template.X,
					y: template.Y + offset,
					paint: paint
				);
				offset += template.LineHeight;
			}
			canvas.Restore();

			using SKImage image = surface.Snapshot();
			using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

			using MemoryStream memoryStream = new();
			data.SaveTo(memoryStream);

			return memoryStream.ToArray();
		}

		private static List<string> WrapWords(string text, float maxWidth, SKPaint paint) {
			string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			List<string> lines = [];
			bool firstWord = true;
			string line = "";
			foreach (string word in words) {
				// Do not wrap first word
				if (firstWord) {
					line = word;
					firstWord = false;
					continue;
				}

				// Measure how wide will it take if we append this word to the current line
				string testLine = line + " " + word;
				SKRect bound = new();
				paint.MeasureText(testLine, ref bound);

				// Wrap
				if (bound.Width > maxWidth) {
					lines.Add(line);
					line = word;
					continue;
				}

				// Append
				line += " " + word;
			}
			lines.Add(line);

			return lines;
		}
	}
}
