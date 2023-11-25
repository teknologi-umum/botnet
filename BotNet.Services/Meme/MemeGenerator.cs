using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BotNet.Services.Typography;
using SkiaSharp;

namespace BotNet.Services.Meme {
	public class MemeGenerator {
		private readonly BotNetFontService _botNetFontService;

		public MemeGenerator(
			BotNetFontService botNetFontService
		) {
			_botNetFontService = botNetFontService;
		}

		public byte[] CaptionRamad(string text) {
			using Stream templateStream = typeof(MemeGenerator).Assembly.GetManifestResourceStream(Templates.RAMAD.ImageResourceName)!;
			using SKBitmap template = SKBitmap.Decode(templateStream);
			using SKSurface surface = SKSurface.Create(new SKImageInfo(template.Width, template.Height));
			using SKCanvas canvas = surface.Canvas;

			SKRect templateRect = SKRect.Create(template.Width, template.Height);
			canvas.DrawBitmap(
				bitmap: template,
				source: templateRect,
				dest: templateRect
			);

			canvas.Save();
			canvas.RotateDegrees(1.4f);
			using Stream fontStream = _botNetFontService.GetFontStyleById("Inter-Regular").OpenStream();
			using SKTypeface typeface = SKTypeface.FromStream(fontStream);
			using SKPaint paint = new() {
				TextAlign = SKTextAlign.Left,
				Color = new SKColor(0x00, 0x00, 0x00, 0xcc),
				Typeface = typeface,
				TextSize = 17f,
				IsAntialias = true
			};
			float offset = 0f;
			foreach (string line in WrapWords(text, 110f, paint)) {
				canvas.DrawText(
					text: line,
					x: 120f,
					y: 100f + offset,
					paint: paint
				);
				offset += 20f; // line height
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
			List<string> lines = new();
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
