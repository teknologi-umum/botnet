using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI {
	public class AssistantBot {
		private readonly OpenAIClient _openAIClient;

		public AssistantBot(
			OpenAIClient openAIClient
		) {
			_openAIClient = openAIClient;
		}

		public Task<string> AskSomethingAsync(string name, string question, CancellationToken cancellationToken) {
			string prompt = $"The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly.\n\n"
				+ $"{name}: Halo, apa kabar?\n"
				+ "AI: Saya adalah AI yang diciptakan oleh TEKNUM. Apakah ada yang bisa saya bantu?\n\n"
				+ $"{name}: {question}\n"
				+ "AI: ";
			return _openAIClient.AutocompleteAsync(
				engine: "davinci-codex",
				prompt: prompt,
				stop: new[] { $"{name}:" },
				maxTokens: 128,
				frequencyPenalty: 0.0,
				presencePenalty: 0.0,
				temperature: 0.2,
				cancellationToken: cancellationToken
			);
		}
	}
}
