using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI.Skills {
	public class CodeGenerator(
		OpenAIClient openAIClient
	) {
		private readonly OpenAIClient _openAIClient = openAIClient;

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
			string code = await _openAIClient.AutocompleteAsync(
				engine: "code-davinci-002",
				prompt: prompt,
				stop: ["/* Command:"],
				maxTokens: 256,
				frequencyPenalty: 0.5,
				presencePenalty: 0.0,
				temperature: 0.0,
				topP: 1.0,
				cancellationToken: cancellationToken
			);
			return code;
		}
	}
}
