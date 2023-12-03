using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI.Skills {
	public class CodeExplainer(
		OpenAIClient openAIClient
	) {
		private readonly OpenAIClient _openAIClient = openAIClient;

		public async Task<string> ExplainCodeInEnglishAsync(string code, CancellationToken cancellationToken) {
			string prompt = code + "\n\n\"\"\"\nHere's what the above code is doing:\n1.";
			string explanation = await _openAIClient.AutocompleteAsync(
				engine: "code-davinci-002",
				prompt: prompt,
				stop: ["\"\"\""],
				maxTokens: 128,
				frequencyPenalty: 0.5,
				presencePenalty: 0.0,
				temperature: 0.0,
				topP: 1.0,
				cancellationToken: cancellationToken
			);
			return "1." + explanation;
		}

		public async Task<string> ExplainCodeInIndonesianAsync(string code, CancellationToken cancellationToken) {
			string prompt = code + "\n\n\"\"\"\nYang dilakukan kode di atas adalah sebagai berikut:\n1.";
			string explanation = await _openAIClient.AutocompleteAsync(
				engine: "code-davinci-002",
				prompt: prompt,
				stop: ["\"\"\""],
				maxTokens: 128,
				frequencyPenalty: 0.5,
				presencePenalty: 0.0,
				temperature: 0.0,
				topP: 1.0,
				cancellationToken: cancellationToken
			);
			return "1." + explanation;
		}
	}
}
