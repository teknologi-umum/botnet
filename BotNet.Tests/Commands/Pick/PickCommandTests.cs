using BotNet.Commands.Pick;
using Shouldly;
using Xunit;

namespace BotNet.Tests.Commands.Pick {
	public class PickCommandTests {
		[Theory]
		[InlineData("pizza sushi burger", new[] { "pizza", "sushi", "burger" })]
		[InlineData("  apple   banana   orange  ", new[] { "apple", "banana", "orange" })]
		[InlineData("one two", new[] { "one", "two" })]
		[InlineData("a b c d e", new[] { "a", "b", "c", "d", "e" })]
		public void ParseOptions_SpaceSeparated_ReturnsCorrectOptions(string input, string[] expected) {
			// Act
			string[] result = PickCommand.ParseOptions(input);

			// Assert
			result.ShouldBe(expected);
		}

		[Theory]
		[InlineData("\"pizza hut\" domino mcdonalds", new[] { "pizza hut", "domino", "mcdonalds" })]
		[InlineData("\"new york\" london \"los angeles\"", new[] { "new york", "london", "los angeles" })]
		[InlineData("\"option one\" \"option two\"", new[] { "option one", "option two" })]
		[InlineData("normal \"quoted option\" another", new[] { "normal", "quoted option", "another" })]
		public void ParseOptions_QuotedStrings_ReturnsCorrectOptions(string input, string[] expected) {
			// Act
			string[] result = PickCommand.ParseOptions(input);

			// Assert
			result.ShouldBe(expected);
		}

		[Theory]
		[InlineData("\"apples, oranges\" bananas cherries", new[] { "apples, oranges", "bananas", "cherries" })]
		[InlineData("\"item, with, commas\" \"another, item\" simple", new[] { "item, with, commas", "another, item", "simple" })]
		[InlineData("\"new york, ny\" chicago \"boston, ma\"", new[] { "new york, ny", "chicago", "boston, ma" })]
		public void ParseOptions_QuotedStringsWithCommas_PreservesCommas(string input, string[] expected) {
			// Act
			string[] result = PickCommand.ParseOptions(input);

			// Assert
			result.ShouldBe(expected);
		}

		[Theory]
		[InlineData("pizza, sushi, burger", true)]
		[InlineData("apple,banana,orange", true)]
		[InlineData("one, two, three", true)]
		[InlineData("no commas here", false)]
		[InlineData("\"quoted, with comma\" but no outside", false)]
		[InlineData("\"first, option\", second", true)]
		public void ContainsCommaOutsideQuotes_DetectsCommasCorrectly(string input, bool expected) {
			// Act
			bool result = PickCommand.ContainsCommaOutsideQuotes(input);

			// Assert
			result.ShouldBe(expected);
		}

		[Theory]
		[InlineData("pizza, sushi, burger", new[] { "pizza", " sushi", " burger" })]
		[InlineData("apple,banana,orange", new[] { "apple", "banana", "orange" })]
		[InlineData("one, two, three", new[] { "one", " two", " three" })]
		[InlineData("a,b", new[] { "a", "b" })]
		public void SplitByCommaOutsideQuotes_CommaSeparated_SplitsCorrectly(string input, string[] expected) {
			// Act
			string[] result = PickCommand.SplitByCommaOutsideQuotes(input);

			// Assert
			result.ShouldBe(expected);
		}

		[Theory]
		[InlineData("\"pizza, with cheese\", burger, sushi", new[] { "\"pizza, with cheese\"", " burger", " sushi" })]
		[InlineData("\"new york, ny\", \"los angeles, ca\", chicago", new[] { "\"new york, ny\"", " \"los angeles, ca\"", " chicago" })]
		[InlineData("\"first, option\",second,\"third, option\"", new[] { "\"first, option\"", "second", "\"third, option\"" })]
		public void SplitByCommaOutsideQuotes_QuotedWithCommas_IgnoresCommasInsideQuotes(string input, string[] expected) {
			// Act
			string[] result = PickCommand.SplitByCommaOutsideQuotes(input);

			// Assert
			result.ShouldBe(expected);
		}

		[Theory]
		[InlineData("\"only one quoted option\"")]
		[InlineData("single")]
		[InlineData("")]
		[InlineData("   ")]
		public void ParseOptions_LessThanTwoOptions_ReturnsArray(string input) {
			// Act
			string[] result = PickCommand.ParseOptions(input);

			// Assert
			// The validation for minimum 2 options happens in FromSlashCommand, not in ParseOptions
			// ParseOptions just returns what it can parse
			result.Length.ShouldBeLessThan(2);
		}

		[Theory]
		[InlineData("\"unclosed quote", new[] { "unclosed quote" })]
		[InlineData("\"quote\" normal \"unclosed", new[] { "quote", "normal", "unclosed" })]
		public void ParseOptions_UnclosedQuotes_HandlesGracefully(string input, string[] expected) {
			// Act
			string[] result = PickCommand.ParseOptions(input);

			// Assert
			result.ShouldBe(expected);
		}

		[Theory]
		[InlineData("\"\"\"\" \"\"", new string[] { })]
		[InlineData("     ", new string[] { })]
		[InlineData("\"\" \"\" \"\"", new string[] { })]
		public void ParseOptions_EmptyQuotesAndSpaces_ReturnsEmptyOrFiltered(string input, string[] expected) {
			// Act
			string[] result = PickCommand.ParseOptions(input);

			// Assert
			result.ShouldBe(expected);
		}

		[Theory]
		[InlineData("option1  \"option 2\"  option3", new[] { "option1", "option 2", "option3" })]
		[InlineData("  \"  spaces inside  \"  outside  ", new[] { "spaces inside", "outside" })]
		public void ParseOptions_MixedSpacing_TrimsCorrectly(string input, string[] expected) {
			// Act
			string[] result = PickCommand.ParseOptions(input);

			// Assert
			result.ShouldBe(expected);
		}
	}
}
