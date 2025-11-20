using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BotNet.Services.Typography;
using SkiaSharp;

namespace BotNet.Services.TechEmpower {
	public sealed class BenchmarkChartRenderer(
		BotNetFontService botNetFontService
	) {
		private const int ChartWidth = 1200;
		private const int ChartHeight = 100; // Base height, will be adjusted based on number of results
		private const int BarHeight = 40;
		private const int BarSpacing = 10;
		private const int LeftMargin = 300;
		private const int RightMargin = 150;
		private const int TopMargin = 60;
		private const int BottomMargin = 40;

		// TechEmpower standard color codes for common languages
		private static readonly Dictionary<string, SKColor> LanguageColors = new(StringComparer.OrdinalIgnoreCase) {
			{ "c#", new SKColor(0x68, 0x21, 0x7A) },        // Purple
			{ "c++", new SKColor(0x00, 0x59, 0x9C) },       // Blue
			{ "java", new SKColor(0xB0, 0x72, 0x19) },      // Orange
			{ "javascript", new SKColor(0xF7, 0xDF, 0x1E) }, // Yellow
			{ "python", new SKColor(0x30, 0x77, 0xAA) },    // Blue
			{ "go", new SKColor(0x00, 0xAD, 0xD8) },        // Cyan
			{ "rust", new SKColor(0xDE, 0xA5, 0x84) },      // Rust color
			{ "php", new SKColor(0x77, 0x7B, 0xB4) },       // Purple
			{ "ruby", new SKColor(0xCC, 0x00, 0x00) },      // Red
			{ "kotlin", new SKColor(0x7F, 0x52, 0xFF) },    // Purple
			{ "swift", new SKColor(0xF0, 0x50, 0x2D) },     // Orange
			{ "scala", new SKColor(0xDC, 0x32, 0x2F) },     // Red
			{ "dart", new SKColor(0x00, 0xB4, 0xAB) },      // Teal
			{ "typescript", new SKColor(0x30, 0x7D, 0xC1) }, // Blue
			{ "c", new SKColor(0x55, 0x55, 0x55) },         // Gray
			{ "elixir", new SKColor(0x6E, 0x4A, 0x7E) },    // Purple
			{ "clojure", new SKColor(0x5B, 0x81, 0xAC) },   // Blue
			{ "erlang", new SKColor(0xA9, 0x02, 0x33) },    // Red
			{ "haskell", new SKColor(0x5D, 0x4F, 0x85) },   // Purple
			{ "nim", new SKColor(0xFF, 0xC2, 0x00) },       // Yellow
		};

		public byte[] RenderBenchmarkChart(BenchmarkResult[] results) {
			if (results == null || results.Length == 0) {
				throw new ArgumentException("Results cannot be null or empty", nameof(results));
			}

			// Sort by score descending
			BenchmarkResult[] sortedResults = results.OrderByDescending(r => r.Score).ToArray();

			// Calculate chart dimensions
			int chartContentHeight = (BarHeight + BarSpacing) * sortedResults.Length + BarSpacing;
			int totalHeight = TopMargin + chartContentHeight + BottomMargin;

			using SKBitmap bitmap = new(ChartWidth, totalHeight);
			using SKSurface surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height));
			using SKCanvas canvas = surface.Canvas;

			// Clear with white background
			canvas.Clear(SKColors.White);

			// Load font
			using Stream fontStream = botNetFontService.GetFontStyleById("JetBrainsMonoNL-Regular").OpenStream();
			using SKTypeface typeface = SKTypeface.FromStream(fontStream);

			// Draw title
			using (SKPaint titlePaint = new() {
				Color = SKColors.Black,
				TextSize = 24f,
				IsAntialias = true,
				Typeface = typeface
			}) {
				canvas.DrawText("TechEmpower Framework Benchmarks", 20, 35, titlePaint);
			}

			// Find max score for scaling
			double maxScore = sortedResults[0].Score;

			// Draw bars
			float yPosition = TopMargin + BarSpacing;
			foreach (BenchmarkResult result in sortedResults) {
				DrawBar(canvas, typeface, result, yPosition, maxScore);
				yPosition += BarHeight + BarSpacing;
			}

			using SKImage image = surface.Snapshot();
			using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

			using MemoryStream memoryStream = new();
			data.SaveTo(memoryStream);

			return memoryStream.ToArray();
		}

		private void DrawBar(SKCanvas canvas, SKTypeface typeface, BenchmarkResult result, float y, double maxScore) {
			// Calculate bar width
			double barWidthRatio = result.Score / maxScore;
			float maxBarWidth = ChartWidth - LeftMargin - RightMargin;
			float barWidth = (float)(maxBarWidth * barWidthRatio);

			// Get color for language
			SKColor barColor = GetLanguageColor(result.Language);

			// Draw bar
			using (SKPaint barPaint = new() {
				Color = barColor,
				Style = SKPaintStyle.Fill,
				IsAntialias = true
			}) {
				SKRect barRect = new(LeftMargin, y, LeftMargin + barWidth, y + BarHeight);
				canvas.DrawRoundRect(barRect, 4, 4, barPaint);
			}

			// Draw framework name (left side)
			using (SKPaint labelPaint = new() {
				Color = SKColors.Black,
				TextSize = 14f,
				IsAntialias = true,
				Typeface = typeface
			}) {
				string label = $"{result.Framework} ({result.Language})";
				if (label.Length > 40) {
					label = label.Substring(0, 37) + "...";
				}
				canvas.DrawText(label, 10, y + BarHeight / 2 + 5, labelPaint);
			}

			// Draw score (right side of bar)
			using (SKPaint scorePaint = new() {
				Color = SKColors.Black,
				TextSize = 14f,
				IsAntialias = true,
				Typeface = typeface
			}) {
				string scoreText = $"{result.Score:N0} req/s";
				canvas.DrawText(scoreText, LeftMargin + barWidth + 10, y + BarHeight / 2 + 5, scorePaint);
			}

			// Draw rank badge
			using (SKPaint rankPaint = new() {
				Color = new SKColor(100, 100, 100),
				TextSize = 12f,
				IsAntialias = true,
				Typeface = typeface
			}) {
				string rankText = $"#{result.Rank}";
				float rankX = LeftMargin + barWidth - 40;
				if (barWidth > 60) {
					canvas.DrawText(rankText, rankX, y + BarHeight / 2 + 4, rankPaint);
				}
			}
		}

		private static SKColor GetLanguageColor(string language) {
			if (LanguageColors.TryGetValue(language, out SKColor color)) {
				return color;
			}

			// Generate a consistent color based on language name hash
			int hash = language.GetHashCode();
			byte r = (byte)((hash & 0xFF0000) >> 16);
			byte g = (byte)((hash & 0x00FF00) >> 8);
			byte b = (byte)(hash & 0x0000FF);

			// Ensure the color isn't too light or too dark
			if (r + g + b < 150) {
				r = (byte)(r + 100);
				g = (byte)(g + 100);
				b = (byte)(b + 100);
			}
			if (r + g + b > 600) {
				r = (byte)(r / 2);
				g = (byte)(g / 2);
				b = (byte)(b / 2);
			}

			return new SKColor(r, g, b);
		}
	}
}
