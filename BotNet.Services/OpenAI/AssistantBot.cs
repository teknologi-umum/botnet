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
				+ "Human: Hello, how are you?\n"
				+ "AI: I am an AI created by TEKNUM. How can I help you today?\n\n"
				+ $"Human: {question}\n"
				+ "AI: ";
			return _openAIClient.DavinciCodexAutocompleteAsync(
				source: prompt,
				stop: new[] { "Human:" },
				maxRecursion: 0,
				cancellationToken: cancellationToken
			);
		}
	}
}
