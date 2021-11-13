using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI {
	public class CodeExplainer {
		private readonly OpenAIClient _openAIClient;

		public CodeExplainer(
			OpenAIClient openAIClient
		) {
			_openAIClient = openAIClient;
		}

		public async Task<string> ExplainCodeInEnglishAsync(string code, CancellationToken cancellationToken) {
			string prompt = code + "\n\n\"\"\"\nHere's what the above code is doing:\n1.";
			string explanation = await _openAIClient.AutocompleteAsync(
				engine: "davinci-codex",
				prompt: prompt,
				stop: new[] { "\"\"\"" },
				maxTokens: 128,
				frequencyPenalty: 0.5,
				presencePenalty: 0.0,
				temperature: 0.0,
				cancellationToken: cancellationToken
			);
			return "1." + explanation;
		}

		public async Task<string> ExplainCodeInIndonesianAsync(string code, CancellationToken cancellationToken) {
			string prompt = code + "\n\n\"\"\"\nYang dilakukan kode di atas adalah sebagai berikut:\n1.";
			string explanation = await _openAIClient.AutocompleteAsync(
				engine: "davinci-codex",
				prompt: prompt,
				stop: new[] { "\"\"\"" },
				maxTokens: 128,
				frequencyPenalty: 0.5,
				presencePenalty: 0.0,
				temperature: 0.0,
				cancellationToken: cancellationToken
			);
			return "1." + explanation;
		}
	}
}
