using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI {
	public class CodeGenerator {
		private readonly OpenAIClient _openAIClient;

		public CodeGenerator(
			OpenAIClient openAIClient
		) {
			_openAIClient = openAIClient;
		}

		public async Task<string> GenerateJavaScriptCodeAsync(string instructions, CancellationToken cancellationToken) {
			string prompt = "<|endoftext|>"
				+ "/* I start with a blank HTML page, and incrementally modify it via <script> injection. Written for Chrome. */\n"
				+ "/* Command: Add \"Hello World\", by adding an HTML DOM node */\n"
				+ "var helloWorld = document.createElement('div');\n"
				+ "document.body.appendChild(helloWorld);\n"
				+ "/* Command: Clear the page. */\n"
				+ "while (document.body.firstChild) {\n"
				+ "  document.body.removeChild(document.body.firstChild);\n"
				+ "}\n\n"
				+ string.Join('\n', instructions.Split('\n').Select(instruction => $"/* {instruction} */")) + "\n";
			string code = await _openAIClient.DavinciCodexAutocompleteAsync(
				prompt: prompt,
				stop: new[] { "/* Command:" },
				maxTokens: 512,
				cancellationToken: cancellationToken
			);
			return code;
		}
	}
}
