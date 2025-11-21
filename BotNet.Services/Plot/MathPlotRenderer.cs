using System;
using System.IO;
using BotNet.Services.Typography;
using DynamicExpresso;
using SkiaSharp;

namespace BotNet.Services.Plot {
	public class MathPlotRenderer(
		BotNetFontService botNetFontService
	) {
		private const int Width = 800;
		private const int Height = 800;
		private const int Padding = 60;
		private const float GridSpacing = 40f;

		public byte[] RenderPlot(string expression) {
			if (string.IsNullOrWhiteSpace(expression)) {
				throw new ArgumentNullException(nameof(expression));
			}

			using SKBitmap bitmap = new(Width, Height);
			using SKSurface surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height));
			using SKCanvas canvas = surface.Canvas;

			// Clear with white background
			canvas.Clear(SKColors.White);

			// Parse the expression
			ExpressionResult result = ParseExpression(expression);

			// Draw grid and axes
			DrawGrid(canvas);
			DrawAxes(canvas);

			// Draw the plot
			if (result.IsImplicit) {
				DrawImplicitFunction(canvas, result.LeftSide, result.RightSide);
			} else {
				DrawExplicitFunction(canvas, result.FunctionY);
			}

			// Draw expression label
			DrawExpressionLabel(canvas, expression);

			using SKImage image = surface.Snapshot();
			using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

			using MemoryStream memoryStream = new();
			data.SaveTo(memoryStream);

			return memoryStream.ToArray();
		}

		private void DrawGrid(SKCanvas canvas) {
			using SKPaint gridPaint = new() {
				Color = new SKColor(220, 220, 220),
				StrokeWidth = 1,
				IsAntialias = true,
				Style = SKPaintStyle.Stroke
			};

			// Vertical grid lines
			for (float x = Padding; x < Width - Padding; x += GridSpacing) {
				canvas.DrawLine(x, Padding, x, Height - Padding, gridPaint);
			}

			// Horizontal grid lines
			for (float y = Padding; y < Height - Padding; y += GridSpacing) {
				canvas.DrawLine(Padding, y, Width - Padding, y, gridPaint);
			}
		}

		private void DrawAxes(SKCanvas canvas) {
			using SKPaint axisPaint = new() {
				Color = SKColors.Black,
				StrokeWidth = 2,
				IsAntialias = true,
				Style = SKPaintStyle.Stroke
			};

			float centerX = Width / 2f;
			float centerY = Height / 2f;

			// X-axis
			canvas.DrawLine(Padding, centerY, Width - Padding, centerY, axisPaint);

			// Y-axis
			canvas.DrawLine(centerX, Padding, centerX, Height - Padding, axisPaint);

			// Draw arrow heads
			using SKPaint arrowPaint = new() {
				Color = SKColors.Black,
				StrokeWidth = 2,
				IsAntialias = true,
				Style = SKPaintStyle.Fill
			};

			// X-axis arrow
			SKPath xArrow = new();
			xArrow.MoveTo(Width - Padding, centerY);
			xArrow.LineTo(Width - Padding - 10, centerY - 5);
			xArrow.LineTo(Width - Padding - 10, centerY + 5);
			xArrow.Close();
			canvas.DrawPath(xArrow, arrowPaint);

			// Y-axis arrow
			SKPath yArrow = new();
			yArrow.MoveTo(centerX, Padding);
			yArrow.LineTo(centerX - 5, Padding + 10);
			yArrow.LineTo(centerX + 5, Padding + 10);
			yArrow.Close();
			canvas.DrawPath(yArrow, arrowPaint);
		}

		private void DrawExplicitFunction(SKCanvas canvas, Func<double, double> func) {
			using SKPaint linePaint = new() {
				Color = new SKColor(0, 100, 200),
				StrokeWidth = 3,
				IsAntialias = true,
				Style = SKPaintStyle.Stroke
			};

			using SKPath path = new();
			bool pathStarted = false;

			int plotWidth = Width - 2 * Padding;
			int plotHeight = Height - 2 * Padding;

			// Scale: -10 to 10 on both axes
			double xMin = -10;
			double xMax = 10;
			double yMin = -10;
			double yMax = 10;

			for (int px = 0; px < plotWidth; px++) {
				double x = xMin + (xMax - xMin) * px / plotWidth;
				double y = func(x);

				if (double.IsNaN(y) || double.IsInfinity(y) || y < yMin || y > yMax) {
					pathStarted = false;
					continue;
				}

				float screenX = Padding + px;
				float screenY = Padding + plotHeight - (float)((y - yMin) / (yMax - yMin) * plotHeight);

				if (!pathStarted) {
					path.MoveTo(screenX, screenY);
					pathStarted = true;
				} else {
					path.LineTo(screenX, screenY);
				}
			}

			canvas.DrawPath(path, linePaint);
		}

		private void DrawImplicitFunction(SKCanvas canvas, Func<double, double, double> leftSide, Func<double, double, double> rightSide) {
			using SKPaint pointPaint = new() {
				Color = new SKColor(0, 100, 200),
				IsAntialias = true,
				Style = SKPaintStyle.Fill
			};

			int plotWidth = Width - 2 * Padding;
			int plotHeight = Height - 2 * Padding;

			// Scale: -10 to 10 on both axes
			double xMin = -10;
			double xMax = 10;
			double yMin = -10;
			double yMax = 10;

			double tolerance = 0.15;

			// Sample the domain and plot points where the equation is satisfied
			for (int px = 0; px < plotWidth; px++) {
				for (int py = 0; py < plotHeight; py++) {
					double x = xMin + (xMax - xMin) * px / plotWidth;
					double y = yMax - (yMax - yMin) * py / plotHeight;

					try {
						double left = leftSide(x, y);
						double right = rightSide(x, y);

						if (double.IsNaN(left) || double.IsInfinity(left) ||
						    double.IsNaN(right) || double.IsInfinity(right)) {
							continue;
						}

						if (Math.Abs(left - right) < tolerance) {
							float screenX = Padding + px;
							float screenY = Padding + py;
							canvas.DrawCircle(screenX, screenY, 1.5f, pointPaint);
						}
					} catch {
						// Skip points that cause evaluation errors
					}
				}
			}
		}

		private void DrawExpressionLabel(SKCanvas canvas, string expression) {
			using Stream fontStream = botNetFontService.GetFontStyleById("Inter-Regular").OpenStream();
			using SKTypeface typeface = SKTypeface.FromStream(fontStream);
			using SKFont font = new(typeface, 20);
			using SKPaint paint = new() {
				Color = new SKColor(50, 50, 50),
				IsAntialias = true
			};

			canvas.DrawText(expression, 10, 30, SKTextAlign.Left, font, paint);
		}

		private ExpressionResult ParseExpression(string expression) {
			// Remove spaces
			string expr = expression.Replace(" ", "").ToLowerInvariant();

			// Check if it's an equation (contains =)
			if (expr.Contains('=')) {
				string[] parts = expr.Split('=');
				if (parts.Length != 2) {
					throw new ArgumentException("Invalid equation format. Use format like 'x + y = 1'");
				}

				string leftExpr = parts[0];
				string rightExpr = parts[1];

				// Compile the expressions once for performance
				Lambda leftLambda = CompileExpression(leftExpr);
				Lambda rightLambda = CompileExpression(rightExpr);

				return new ExpressionResult {
					IsImplicit = true,
					LeftSide = (x, y) => {
						try {
							return Convert.ToDouble(leftLambda.Invoke(new Parameter("x", x), new Parameter("y", y)));
						} catch {
							return double.NaN;
						}
					},
					RightSide = (x, y) => {
						try {
							return Convert.ToDouble(rightLambda.Invoke(new Parameter("x", x), new Parameter("y", y)));
						} catch {
							return double.NaN;
						}
					}
				};
			}

			// Otherwise, treat as y = f(x)
			Lambda lambda = CompileExpression(expr);
			return new ExpressionResult {
				IsImplicit = false,
				FunctionY = x => {
					try {
						return Convert.ToDouble(lambda.Invoke(new Parameter("x", x), new Parameter("y", 0.0)));
					} catch {
						return double.NaN;
					}
				}
			};
		}

		private Lambda CompileExpression(string expr) {
			// Handle implicit multiplication (e.g., 2x -> 2*x)
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(\d)([xy])", "$1*$2");

			// Map function names to Math.* methods using word boundaries
			// Use negative lookbehind to avoid matching already-replaced patterns
			// Order matters: longer patterns first to avoid partial matches
			
			// Logarithmic functions (longer patterns first)
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\blog10\b", "Math.Log10", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\blog2\b", "Math.Log2", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bln\b", "Math.Log", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\blog\b", "Math.Log", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			
			// Hyperbolic trig functions (must come before basic trig to avoid sinh->Math.Sinh->Math.SinMath.H)
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\basinh\b", "Math.Asinh", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bacosh\b", "Math.Acosh", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\batanh\b", "Math.Atanh", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bsinh\b", "Math.Sinh", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bcosh\b", "Math.Cosh", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\btanh\b", "Math.Tanh", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			
			// Inverse trig functions (before basic trig)
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\basin\b", "Math.Asin", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bacos\b", "Math.Acos", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\batan\b", "Math.Atan", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			
			// Reciprocal trig functions (before basic trig)
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"\basec\b", "Asec", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"\bacsc\b", "Acsc", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"\bacot\b", "Acot", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"\bsec\b", "Sec", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"\bcsc\b", "Csc", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"\bcot\b", "Cot", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			
			// Basic trigonometric functions
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bsin\b", "Math.Sin", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bcos\b", "Math.Cos", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\btan\b", "Math.Tan", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			
			// Arithmetic functions
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bsqrt\b", "Math.Sqrt", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bcbrt\b", "Math.Cbrt", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bpow\b", "Math.Pow", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bexp\b", "Math.Exp", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\babs\b", "Math.Abs", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			
			// Rounding and comparison functions
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bmax\b", "Math.Max", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bmin\b", "Math.Min", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bround\b", "Math.Round", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bfloor\b", "Math.Floor", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bceil\b", "Math.Ceiling", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"\bmod\b", "Mod", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"\bclamp\b", "Clamp", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"\brand\b", "Rand", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"\blerp\b", "Lerp", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			
			// Angle conversion functions
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"\brad\b", "Rad", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"\bdeg\b", "Deg", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			
			// Constants
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\bpi\b", "Math.PI", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"(?<!Math\.)\be\b", "Math.E", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			expr = System.Text.RegularExpressions.Regex.Replace(expr, @"\binf\b", "double.PositiveInfinity", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			
			// Create interpreter with Math references
			Interpreter interpreter = new();
			interpreter.Reference(typeof(Math));
			
			// Register custom functions
			interpreter.SetFunction("Mod", (Func<double, double, double>)((a, b) => a % b));
			interpreter.SetFunction("Clamp", (Func<double, double, double, double>)((value, min, max) => Math.Max(min, Math.Min(max, value))));
			interpreter.SetFunction("Rand", (Func<double>)(() => Random.Shared.NextDouble()));
			interpreter.SetFunction("Lerp", (Func<double, double, double, double>)((a, b, t) => a + (b - a) * t));
			
			// Reciprocal trigonometric functions
			interpreter.SetFunction("Sec", (Func<double, double>)(x => 1.0 / Math.Cos(x)));
			interpreter.SetFunction("Csc", (Func<double, double>)(x => 1.0 / Math.Sin(x)));
			interpreter.SetFunction("Cot", (Func<double, double>)(x => 1.0 / Math.Tan(x)));
			interpreter.SetFunction("Asec", (Func<double, double>)(x => Math.Acos(1.0 / x)));
			interpreter.SetFunction("Acsc", (Func<double, double>)(x => Math.Asin(1.0 / x)));
			interpreter.SetFunction("Acot", (Func<double, double>)(x => Math.Atan(1.0 / x)));
			
			// Angle conversion functions
			interpreter.SetFunction("Rad", (Func<double, double>)(deg => deg * Math.PI / 180.0));
			interpreter.SetFunction("Deg", (Func<double, double>)(rad => rad * 180.0 / Math.PI));

			// Parse the expression with x and y as parameters
			return interpreter.Parse(expr, new Parameter("x", typeof(double)), new Parameter("y", typeof(double)));
		}

		private class ExpressionResult {
			public bool IsImplicit { get; set; }
			public Func<double, double>? FunctionY { get; set; }
			public Func<double, double, double>? LeftSide { get; set; }
			public Func<double, double, double>? RightSide { get; set; }
		}
	}
}
