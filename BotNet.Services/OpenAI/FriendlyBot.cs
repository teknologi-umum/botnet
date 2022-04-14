using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI {
	public class ConversationBot {
		private readonly OpenAIClient _openAIClient;

		public ConversationBot(
			OpenAIClient openAIClient
		) {
			_openAIClient = openAIClient;
		}

		public Task<string> ChatAsync(string name, string question, CancellationToken cancellationToken) {
			string prompt = $"The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly.\n\n"
				+ $"{name}: Hello, how are you?\n"
				+ "AI: I am an AI created by TEKNUM. How can I help you today?\n\n"
				+ $"{name}: {question}\n"
				+ "AI: ";
			return _openAIClient.AutocompleteAsync(
				engine: "text-davinci-002",
				prompt: prompt,
				stop: new[] { $"{name}:" },
				maxTokens: 128,
				frequencyPenalty: 0.0,
				presencePenalty: 0.0,
				temperature: 0.0,
				cancellationToken: cancellationToken
			);
		}

		public Task<string> RespondToThreadAsync(string name, string question, ImmutableList<(string Sender, string Text)> thread, CancellationToken cancellationToken) {
			string prompt = $"The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly.\n\n"
				+ $"{name}: Hello, how are you?\n"
				+ "AI: I am an AI created by TEKNUM. How can I help you today?\n\n";
			foreach((string sender, string text) in thread) {
				prompt += $"{sender}: {text}\n";
				if (sender == "AI") prompt += "\n";
			}
			prompt +=
				$"{name}: {question}\n"
				+ "AI: ";
			return _openAIClient.AutocompleteAsync(
				engine: "text-davinci-002",
				prompt: prompt,
				stop: new[] { $"{name}:" },
				maxTokens: 128,
				frequencyPenalty: 0.0,
				presencePenalty: 0.0,
				temperature: 0.0,
				cancellationToken: cancellationToken
			);
		}
	}
}
