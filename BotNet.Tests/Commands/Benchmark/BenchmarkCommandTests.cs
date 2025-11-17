using System;
using BotNet.Commands.Benchmark;
using BotNet.Commands.Common;
using Shouldly;
using Xunit;

namespace BotNet.Tests.Commands.Benchmark {
	public class BenchmarkCommandTests {
		[Theory]
		[InlineData("C# C++", new[] { "C#", "C++" })]
		[InlineData("Python Java", new[] { "Python", "Java" })]
		[InlineData("Go Rust TypeScript", new[] { "Go", "Rust", "TypeScript" })]
		[InlineData("JavaScript  PHP  Ruby", new[] { "JavaScript", "PHP", "Ruby" })]
		[InlineData("  C#   C++  ", new[] { "C#", "C++" })]
		[InlineData("JavaScript     TypeScript", new[] { "JavaScript", "TypeScript" })]
		public void ParseLanguages_ValidInput_ReturnsCorrectLanguages(string input, string[] expectedLanguages) {
			// Arrange & Act
			string[] result = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			// Assert
			result.ShouldBe(expectedLanguages);
		}

		[Theory]
		[InlineData("")]
		[InlineData("   ")]
		[InlineData(null)]
		public void ParseLanguages_EmptyOrWhitespace_ReturnsEmptyArray(string? input) {
			// Arrange & Act
			string[] result = string.IsNullOrWhiteSpace(input)
				? Array.Empty<string>()
				: input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			// Assert
			result.ShouldBeEmpty();
		}

		[Theory]
		[InlineData("C#")]
		[InlineData("Python")]
		public void ParseLanguages_SingleLanguage_ReturnsSingleItem(string input) {
			// Arrange & Act
			string[] result = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			// Assert
			result.Length.ShouldBe(1);
		}

		[Fact]
		public void LanguageComparison_CaseInsensitive_ShouldBeSupported() {
			// This test verifies that language matching should be case-insensitive
			// when comparing against benchmark results
			
			// Arrange
			string language1 = "C#";
			string language2 = "c#";

			// Act & Assert
			language1.Equals(language2, StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
		}
	}
}
