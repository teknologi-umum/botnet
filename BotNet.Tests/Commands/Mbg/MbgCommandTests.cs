using System;
using BotNet.Commands.Mbg;
using Shouldly;
using Xunit;

namespace BotNet.Tests.Commands.Mbg {
	public class MbgCommandTests {
		[Theory]
		[InlineData("1000000", 1_000_000)]
		[InlineData("1,000,000", 1_000_000)]
		[InlineData("100000000", 100_000_000)]
		[InlineData("100,000,000", 100_000_000)]
		[InlineData("10000000000", 10_000_000_000)]
		[InlineData("10,000,000,000", 10_000_000_000)]
		[InlineData("1200000000000", 1_200_000_000_000)]
		[InlineData("1,200,000,000,000", 1_200_000_000_000)]
		public void ParseRupiah_ValidInput_ReturnsParsedAmount(string input, decimal expected) {
			// Act
			decimal result = MbgCommand.ParseRupiah(input);

			// Assert
			result.ShouldBe(expected);
		}

		[Theory]
		[InlineData(1_000_000, "0.07 detik MBG")]
		[InlineData(100_000_000, "7.2 detik MBG")]
		[InlineData(10_000_000_000, "12 menit MBG")]
		[InlineData(10_000_000_000_000, "8 hari 8 jam MBG")]
		public void FormatMbgTime_ExampleAmounts_ReturnsExpectedOutput(decimal rupiahAmount, string expected) {
			// Act
			string result = MbgCommand.FormatMbgTime(rupiahAmount);

			// Assert
			result.ShouldBe(expected);
		}

		[Fact]
		public void FormatMbgTime_OneMbgDay_Returns1HariMBG() {
			// 1 MBG day = 1.2 trillion rupiah
			decimal rupiahAmount = 1_200_000_000_000M;

			// Act
			string result = MbgCommand.FormatMbgTime(rupiahAmount);

			// Assert
			result.ShouldBe("1 hari MBG");
		}

		[Fact]
		public void FormatMbgTime_OneYear_ReturnsTahunMBG() {
			// 1 MBG year = 365 * 1.2 trillion rupiah
			decimal rupiahAmount = 365M * 1_200_000_000_000M;

			// Act
			string result = MbgCommand.FormatMbgTime(rupiahAmount);

			// Assert
			result.ShouldBe("1 tahun MBG");
		}

		[Fact]
		public void FormatMbgTime_MoreThanOneYear_ReturnsTahunAndHariMBG() {
			// 1 MBG year + 5 MBG days
			decimal rupiahAmount = (365M + 5M) * 1_200_000_000_000M;

			// Act
			string result = MbgCommand.FormatMbgTime(rupiahAmount);

			// Assert
			result.ShouldBe("1 tahun 5 hari MBG");
		}

		[Theory]
		[InlineData(50_000_000_000, "1 jam MBG")]
		[InlineData(500_000_000_000, "10 jam MBG")]
		public void FormatMbgTime_HourRange_ReturnsJamMBG(decimal rupiahAmount, string expected) {
			// Act
			string result = MbgCommand.FormatMbgTime(rupiahAmount);

			// Assert
			result.ShouldBe(expected);
		}

		[Theory]
		[InlineData("abc")]
		[InlineData("1.2.3")]
		[InlineData("")]
		public void ParseRupiah_InvalidInput_ThrowsFormatException(string input) {
			// Act & Assert
			Should.Throw<FormatException>(() => MbgCommand.ParseRupiah(input));
		}
	}
}
