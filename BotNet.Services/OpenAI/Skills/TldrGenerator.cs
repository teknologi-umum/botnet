using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI.Skills {
	public class TldrGenerator(
		OpenAIClient openAIClient
	) {
		private readonly OpenAIClient _openAIClient = openAIClient;

		public Task<string> GenerateTldrAsync(string text, CancellationToken cancellationToken) {
			string prompt = $"{text}\n\nTl;dr:\n";
			return _openAIClient.AutocompleteAsync(
				engine: "text-davinci-002",
				prompt: prompt,
				stop: null,
				maxTokens: 60,
				frequencyPenalty: 0.0,
				presencePenalty: 0.0,
				temperature: 0.7,
				topP: 1.0,
				cancellationToken: cancellationToken
			);
		}
	}
}
