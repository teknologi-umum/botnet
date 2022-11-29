using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI {
	public class FriendlyBot {
		private readonly OpenAIClient _openAIClient;

		public FriendlyBot(
			OpenAIClient openAIClient
		) {
			_openAIClient = openAIClient;
		}

		public Task<string> ChatAsync(string callSign, string name, string question, CancellationToken cancellationToken) {
			string prompt = $"The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly.\n\n"
				+ $"{name}: Hello, how are you?\n"
				+ $"{callSign}: I am an AI created by TEKNUM. How can I help you today?\n\n"
				+ $"{name}: {question}\n"
				+ $"{callSign}: ";
			return _openAIClient.AutocompleteAsync(
				engine: "text-davinci-003",
				prompt: prompt,
				stop: new[] { $"{name}:" },
				maxTokens: 128,
				frequencyPenalty: 0.5,
				presencePenalty: 0.6,
				temperature: 0.9,
				topP: 1.0,
				cancellationToken: cancellationToken
			);
		}

		public Task<string> RespondToThreadAsync(string callSign, string name, string question, ImmutableList<(string Sender, string Text)> thread, CancellationToken cancellationToken) {
			string prompt = $"The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly.\n\n"
				+ $"{name}: Hello, how are you?\n"
				+ $"{callSign}: I am an AI created by TEKNUM. How can I help you today?\n\n";
			foreach((string sender, string text) in thread) {
				prompt += $"{sender}: {text}\n";
				if (sender is "AI" or "Pakde") prompt += "\n";
			}
			prompt +=
				$"{name}: {question}\n"
				+ $"{callSign}: ";
			return _openAIClient.AutocompleteAsync(
				engine: "text-davinci-003",
				prompt: prompt,
				stop: new[] { $"{name}:" },
				maxTokens: 128,
				frequencyPenalty: 0.5,
				presencePenalty: 0.6,
				temperature: 0.9,
				topP: 1.0,
				cancellationToken: cancellationToken
			);
		}
	}
}
