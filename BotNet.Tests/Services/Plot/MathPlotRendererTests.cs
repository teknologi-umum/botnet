using System;
using BotNet.Services.Plot;
using BotNet.Services.Typography;
using BotNet.Tests.Assertions;
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
			result1.ShouldNotContainSameImageAs(result2);
		}

		[Fact]
		public void RenderPlot_WithSameExpression_ReturnsSameImage() {
			// Act
			byte[] result1 = _mathPlotRenderer.RenderPlot("x + y = 1");
			byte[] result2 = _mathPlotRenderer.RenderPlot("x + y = 1");

			// Assert
			result1.ShouldContainSameImageAs(result2);
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

		// Tests for arithmetic functions
		[Theory]
		[InlineData("cbrt(x)")]
		[InlineData("sqrt(x)")]
		[InlineData("pow(x, 2)")]
		[InlineData("exp(x)")]
		[InlineData("abs(x)")]
		public void RenderPlot_WithArithmeticFunctions_ReturnsValidImage(string expression) {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot(expression);

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}

		// Tests for logarithmic functions
		[Theory]
		[InlineData("ln(x)")]
		[InlineData("log10(x)")]
		[InlineData("log2(x)")]
		[InlineData("log(x)")]
		public void RenderPlot_WithLogarithmicFunctions_ReturnsValidImage(string expression) {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot(expression);

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}

		// Tests for rounding and comparison functions
		[Theory]
		[InlineData("max(x, 0)")]
		[InlineData("min(x, 5)")]
		[InlineData("round(x)")]
		[InlineData("floor(x)")]
		[InlineData("ceil(x)")]
		[InlineData("mod(x, 2)")]
		[InlineData("clamp(x, -5, 5)")]
		public void RenderPlot_WithRoundingAndComparisonFunctions_ReturnsValidImage(string expression) {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot(expression);

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}

		// Tests for hyperbolic trigonometric functions
		[Theory]
		[InlineData("sinh(x)")]
		[InlineData("cosh(x)")]
		[InlineData("tanh(x)")]
		[InlineData("asinh(x)")]
		[InlineData("acosh(x + 2)")]
		[InlineData("atanh(x / 10)")]
		public void RenderPlot_WithHyperbolicTrigFunctions_ReturnsValidImage(string expression) {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot(expression);

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}

		// Tests for inverse trigonometric functions
		[Theory]
		[InlineData("asin(x / 10)")]
		[InlineData("acos(x / 10)")]
		[InlineData("atan(x)")]
		public void RenderPlot_WithInverseTrigFunctions_ReturnsValidImage(string expression) {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot(expression);

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}

		// Tests for reciprocal trigonometric functions
		[Theory]
		[InlineData("sec(x)")]
		[InlineData("csc(x)")]
		[InlineData("cot(x)")]
		[InlineData("asec(x)")]
		[InlineData("acsc(x)")]
		[InlineData("acot(x)")]
		public void RenderPlot_WithReciprocalTrigFunctions_ReturnsValidImage(string expression) {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot(expression);

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}

		// Tests for angle conversion functions
		[Theory]
		[InlineData("sin(rad(x * 10))")]
		[InlineData("cos(rad(x * 10))")]
		[InlineData("deg(x)")]
		public void RenderPlot_WithAngleConversionFunctions_ReturnsValidImage(string expression) {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot(expression);

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}

		// Tests for mathematical constants
		[Theory]
		[InlineData("sin(x) + pi")]
		[InlineData("exp(x) - e")]
		[InlineData("x * pi")]
		[InlineData("e * x")]
		public void RenderPlot_WithMathematicalConstants_ReturnsValidImage(string expression) {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot(expression);

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}

		// Tests for lerp function
		[Fact]
		public void RenderPlot_WithLerpFunction_ReturnsValidImage() {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot("lerp(-5, 5, (x + 10) / 20)");

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}

		// Test for rand function (note: will be different each time)
		[Fact]
		public void RenderPlot_WithRandFunction_ReturnsValidImage() {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot("rand() + x * 0");

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}

		// Test case insensitivity
		[Theory]
		[InlineData("SIN(x)")]
		[InlineData("COS(x)")]
		[InlineData("SQRT(abs(x))")]
		[InlineData("PI")]
		public void RenderPlot_WithUppercaseFunctions_ReturnsValidImage(string expression) {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot(expression);

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}

		// Test complex expressions combining multiple functions
		[Theory]
		[InlineData("sin(x) + cos(x)")]
		[InlineData("sqrt(abs(x)) * pi")]
		[InlineData("ln(abs(x) + 1) * e")]
		[InlineData("clamp(sin(x), -0.5, 0.5)")]
		public void RenderPlot_WithComplexExpressions_ReturnsValidImage(string expression) {
			// Act
			byte[] result = _mathPlotRenderer.RenderPlot(expression);

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}
	}
}
