using System;
using BotNet.Services.Plot;
using BotNet.Services.Typography;
using Shouldly;
using SkiaSharp;
using Xunit;

namespace BotNet.Tests.Services.Plot {
	public class MathPlotRendererTests {
		private readonly MathPlotRenderer _mathPlotRenderer;

		public MathPlotRendererTests() {
			BotNetFontService fontService = new();
			_mathPlotRenderer = new MathPlotRenderer(fontService);
		}

		[Fact]
		public void RenderPlot_WithSimpleLinearEquation_ReturnsNonEmptyByteArray() {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot("x + y = 1");

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
		}

		[Fact]
		public void RenderPlot_WithSimpleLinearEquation_ReturnsValidPngImage() {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot("x + y = 1");

			// Assert
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
			bitmap.Width.ShouldBeGreaterThan(0);
			bitmap.Height.ShouldBeGreaterThan(0);
		}

		[Fact]
		public void RenderPlot_WithSimpleLinearEquation_ReturnsSquareImage() {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot("x + y = 1");

			// Assert
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.Width.ShouldBe(bitmap.Height);
		}

		[Fact]
		public void RenderPlot_WithExplicitFunction_ReturnsValidImage() {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot("x * x");

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}

		[Fact]
		public void RenderPlot_WithCircleEquation_ReturnsValidImage() {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot("x*x + y*y = 25");

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}

		[Fact]
		public void RenderPlot_WithNullExpression_ThrowsArgumentNullException() {
			// Act & Assert
			Should.Throw<ArgumentNullException>(() => _mathPlotRenderer.RenderPlot(null!));
		}

		[Fact]
		public void RenderPlot_WithEmptyExpression_ThrowsArgumentNullException() {
			// Act & Assert
			Should.Throw<ArgumentNullException>(() => _mathPlotRenderer.RenderPlot(""));
		}

		[Fact]
		public void RenderPlot_WithWhitespaceExpression_ThrowsArgumentNullException() {
			// Act & Assert
			Should.Throw<ArgumentNullException>(() => _mathPlotRenderer.RenderPlot("   "));
		}

		[Fact]
		public void RenderPlot_WithDifferentExpressions_ReturnsDifferentImages() {
			// Act
			byte[] result1 = _mathPlotRenderer.RenderPlot("x + y = 1");
			byte[] result2 = _mathPlotRenderer.RenderPlot("x + y = 5");

			// Assert
			result1.ShouldNotBe(result2);
		}

		[Fact]
		public void RenderPlot_WithSameExpression_ReturnsSameImage() {
			// Act
			byte[] result1 = _mathPlotRenderer.RenderPlot("x + y = 1");
			byte[] result2 = _mathPlotRenderer.RenderPlot("x + y = 1");

			// Assert
			result1.ShouldBe(result2);
		}

		[Fact]
		public void RenderPlot_WithValidExpression_ImageHasWhiteBackground() {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot("x + y = 1");

			// Assert
			using SKBitmap bitmap = SKBitmap.Decode(result);
			// Check corners for white background
			SKColor topLeft = bitmap.GetPixel(0, 0);
			SKColor topRight = bitmap.GetPixel(bitmap.Width - 1, 0);
			SKColor bottomLeft = bitmap.GetPixel(0, bitmap.Height - 1);
			SKColor bottomRight = bitmap.GetPixel(bitmap.Width - 1, bitmap.Height - 1);
			
			topLeft.ShouldBe(SKColors.White);
			topRight.ShouldBe(SKColors.White);
			bottomLeft.ShouldBe(SKColors.White);
			bottomRight.ShouldBe(SKColors.White);
		}
	}
}
