using BotNet.Services.Brainfuck;
using Shouldly;
using Xunit;

namespace BotNet.Tests.Services.Brainfuck {
	public class BrainfuckTranspilerTests {
		[Fact]
		public void CanTranspileStringToBrainfuck() {
			BrainfuckTranspiler transpiler = new();
			string s = "Hello world";
			string fucked = transpiler.TranspileBrainfuck(s);
			fucked.ShouldBe("-[------->+<]>-.-[->+++++<]>++.+++++++..+++.[--->+<]>-----.--[->++++<]>-.--------.+++.------.--------.");
		}

		[Fact]
		public void TranspiledStringShouldFuckToOriginalString() {
			BrainfuckTranspiler transpiler = new();
			string s = "The quick brown fox 12345 jumps over the lazy dog 67890";
			string fucked = transpiler.TranspileBrainfuck(s);
			string unfucked = BrainfuckInterpreter.RunBrainfuck(fucked);
			unfucked.ShouldBe(s);
			unfucked.ShouldNotBe(fucked);
		}
	}
}
