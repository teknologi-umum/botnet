using BotNet.Services.TimeZone;
using Shouldly;
using Xunit;

namespace BotNet.Tests.Services.TimeZone {
	public class TimeZoneServiceTests {
		private readonly TimeZoneService _timeZoneService = new();

		[Theory]
		[InlineData("Jakarta")]
		[InlineData("jakarta")]
		[InlineData("JAKARTA")]
		public void GetTimeInfo_WithJakartaVariants_ReturnsSuccess(string input) {
			// Act
			TimeInfo result = _timeZoneService.GetTimeInfo(input);

			// Assert
			result.Success.ShouldBeTrue();
			result.TimeZoneId.ShouldNotBeNullOrWhiteSpace();
			result.ErrorMessage.ShouldBeNull();
		}

		[Fact]
		public void GetTimeInfo_WithIanaCode_ReturnsSuccess() {
			// Act
			TimeInfo result = _timeZoneService.GetTimeInfo("Asia/Jakarta");

			// Assert
			result.Success.ShouldBeTrue();
			result.TimeZoneId.ShouldNotBeNullOrWhiteSpace();
			result.ErrorMessage.ShouldBeNull();
		}

		[Theory]
		[InlineData("WIB")]
		[InlineData("WITA")]
		[InlineData("WIT")]
		public void GetTimeInfo_WithIndonesianAbbreviations_ReturnsSuccess(string input) {
			// Act
			TimeInfo result = _timeZoneService.GetTimeInfo(input);

			// Assert
			result.Success.ShouldBeTrue();
			result.TimeZoneId.ShouldNotBeNullOrWhiteSpace();
			result.ErrorMessage.ShouldBeNull();
		}

		[Theory]
		[InlineData("GMT+7")]
		[InlineData("UTC+7")]
		[InlineData("GMT-5")]
		[InlineData("UTC-5")]
		[InlineData("+7")]
		[InlineData("-5")]
		[InlineData("+7:00")]
		[InlineData("-5:00")]
		public void GetTimeInfo_WithUtcOffsetFormat_ReturnsSuccess(string input) {
			// Act
			TimeInfo result = _timeZoneService.GetTimeInfo(input);

			// Assert
			result.Success.ShouldBeTrue();
			result.TimeZoneId.ShouldNotBeNullOrWhiteSpace();
			result.ErrorMessage.ShouldBeNull();
		}

		[Fact]
		public void GetTimeInfo_WithShorthandOffset_ReturnsCorrectTimezone() {
			// Act
			TimeInfo result = _timeZoneService.GetTimeInfo("+7");

			// Assert
			result.Success.ShouldBeTrue();
			result.UtcOffset.Hours.ShouldBe(7); // Should map to a UTC+7 timezone
		}

		[Theory]
		[InlineData("CST")]
		[InlineData("ET")]
		public void GetTimeInfo_WithAbbreviations_ReturnsSuccess(string input) {
			// Act
			TimeInfo result = _timeZoneService.GetTimeInfo(input);

			// Assert
			result.Success.ShouldBeTrue();
			result.TimeZoneId.ShouldNotBeNullOrWhiteSpace();
			result.ErrorMessage.ShouldBeNull();
		}

		[Fact]
		public void GetTimeInfo_WithInvalidInput_ReturnsFailure() {
			// Act
			TimeInfo result = _timeZoneService.GetTimeInfo("InvalidCityName12345");

			// Assert
			result.Success.ShouldBeFalse();
			result.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
			result.ErrorMessage.ShouldContain("Could not find time zone");
		}

		[Fact]
		public void GetTimeInfo_WithValidCity_ReturnsCorrectUtcOffset() {
			// Act
			TimeInfo result = _timeZoneService.GetTimeInfo("Jakarta");

			// Assert
			result.Success.ShouldBeTrue();
			result.UtcOffset.Hours.ShouldBe(7); // Jakarta is UTC+7
		}

		[Fact]
		public void GetTimeInfo_WithValidCity_ReturnsValidLocalTime() {
			// Act
			TimeInfo result = _timeZoneService.GetTimeInfo("Jakarta");

			// Assert
			result.Success.ShouldBeTrue();
			result.LocalTime.ShouldNotBe(default);
			result.LocalTime.Year.ShouldBeGreaterThan(2020);
		}

		[Theory]
		[InlineData("Porto-Novo")] // Benin - Africa/Porto-Novo
		[InlineData("Sao Tome")] // São Tomé and Príncipe - Africa/Sao_Tome
		[InlineData("Buenos Aires")] // Argentina - America/Argentina/Buenos_Aires
		[InlineData("Comod Rivadavia")] // Argentina - America/Argentina/ComodRivadavia
		[InlineData("North Dakota")] // USA - America/North_Dakota/Center (all have same offset)
		public void GetTimeInfo_WithVariousCities_ReturnsSuccess(string cityName) {
			// Act
			TimeInfo result = _timeZoneService.GetTimeInfo(cityName);

			// Assert
			result.Success.ShouldBeTrue();
			result.TimeZoneId.ShouldNotBeNullOrWhiteSpace();
			result.LocalTime.ShouldNotBe(default);
		}

		[Theory]
		[InlineData("Indiana")] // Multiple timezones with different offsets (EST/CST)
		[InlineData("Australia")] // Multiple timezones with different offsets (AEST/ACST/AWST)
		public void GetTimeInfo_WithAmbiguousRegions_ReturnsFailure(string regionName) {
			// Act
			TimeInfo result = _timeZoneService.GetTimeInfo(regionName);

			// Assert
			result.Success.ShouldBeFalse();
			result.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
		}
	}
}
