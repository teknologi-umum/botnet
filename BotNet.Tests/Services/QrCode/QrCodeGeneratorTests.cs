using BotNet.Services.QrCode;
using Shouldly;
using SkiaSharp;
using Xunit;

namespace BotNet.Tests.Services.QrCode {
	public class QrCodeGeneratorTests {
		private readonly QrCodeGenerator _qrCodeGenerator = new();

		[Fact]
		public void GenerateQrCode_WithValidUrl_ReturnsNonEmptyByteArray() {
			// Act
			byte[] result = _qrCodeGenerator.GenerateQrCode("https://example.com");

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
		}

		[Fact]
		public void GenerateQrCode_WithValidUrl_ReturnsValidPngImage() {
			// Act
			byte[] result = _qrCodeGenerator.GenerateQrCode("https://example.com");

			// Assert
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
			bitmap.Width.ShouldBeGreaterThan(0);
			bitmap.Height.ShouldBeGreaterThan(0);
		}

		[Fact]
		public void GenerateQrCode_WithValidUrl_ReturnsSquareImage() {
			// Act
			byte[] result = _qrCodeGenerator.GenerateQrCode("https://example.com");

			// Assert
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.Width.ShouldBe(bitmap.Height);
		}

		[Fact]
		public void GenerateQrCode_WithLongUrl_ReturnsValidImage() {
			// Arrange
			string longUrl = "https://example.com/very/long/path/that/contains/many/segments/and/query/parameters?param1=value1&param2=value2&param3=value3";

			// Act
			byte[] result = _qrCodeGenerator.GenerateQrCode(longUrl);

			// Assert
			result.ShouldNotBeNull();
			result.Length.ShouldBeGreaterThan(0);
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bitmap.ShouldNotBeNull();
		}

		[Fact]
		public void GenerateQrCode_WithDifferentUrls_ReturnsDifferentImages() {
			// Act
			byte[] result1 = _qrCodeGenerator.GenerateQrCode("https://example.com");
			byte[] result2 = _qrCodeGenerator.GenerateQrCode("https://different.com");

			// Assert
			result1.ShouldNotBe(result2);
		}

		[Fact]
		public void GenerateQrCode_WithSameUrl_ReturnsSameImage() {
			// Act
			byte[] result1 = _qrCodeGenerator.GenerateQrCode("https://example.com");
			byte[] result2 = _qrCodeGenerator.GenerateQrCode("https://example.com");

			// Assert
			result1.ShouldBe(result2);
		}

		[Fact]
		public void GenerateQrCode_WithValidUrl_ImageHasWhiteBackground() {
			// Act
			byte[] result = _qrCodeGenerator.GenerateQrCode("https://example.com");

			// Assert
			using SKBitmap bitmap = SKBitmap.Decode(result);
			// Check corners for white background (quiet zone)
			SKColor topLeft = bitmap.GetPixel(0, 0);
			SKColor topRight = bitmap.GetPixel(bitmap.Width - 1, 0);
			SKColor bottomLeft = bitmap.GetPixel(0, bitmap.Height - 1);
			SKColor bottomRight = bitmap.GetPixel(bitmap.Width - 1, bitmap.Height - 1);
			
			topLeft.ShouldBe(SKColors.White);
			topRight.ShouldBe(SKColors.White);
			bottomLeft.ShouldBe(SKColors.White);
			bottomRight.ShouldBe(SKColors.White);
		}

		[Fact]
		public void GenerateQrCode_WithValidUrl_ImageContainsBlackPixels() {
			// Act
			byte[] result = _qrCodeGenerator.GenerateQrCode("https://example.com");

			// Assert
			using SKBitmap bitmap = SKBitmap.Decode(result);
			bool hasBlackPixel = false;
			
			// Sample some pixels in the center area (where QR modules should be)
			int centerX = bitmap.Width / 2;
			int centerY = bitmap.Height / 2;
			
			for (int x = centerX - 50; x < centerX + 50 && !hasBlackPixel; x++) {
				for (int y = centerY - 50; y < centerY + 50; y++) {
					if (x >= 0 && x < bitmap.Width && y >= 0 && y < bitmap.Height) {
						SKColor pixel = bitmap.GetPixel(x, y);
						if (pixel == SKColors.Black) {
							hasBlackPixel = true;
							break;
						}
					}
				}
			}
			
			hasBlackPixel.ShouldBeTrue();
		}
	}
}
