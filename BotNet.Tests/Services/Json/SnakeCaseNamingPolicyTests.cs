using BotNet.Services.Json;
using Shouldly;
using Xunit;

namespace BotNet.Tests.Services.Json {
	public class SnakeCaseNamingPolicyTests {
		[Theory]
		[InlineData("", "")]
		[InlineData("A", "a")]
		[InlineData("Aa", "aa")]
		[InlineData("AA", "a_a")]
		[InlineData("AAa", "a_aa")]
		[InlineData("AaA", "aa_a")]
		public void CanConvertPascalCaseToSnakeCase(string pascalCase, string expectedSnakeCase) {
			string snakeCase = new SnakeCaseNamingPolicy().ConvertName(pascalCase);
			snakeCase.ShouldBe(expectedSnakeCase);
		}
	}
}
