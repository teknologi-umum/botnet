using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI {
	public class SarcasticBot {
		private readonly OpenAIClient _openAIClient;

		public SarcasticBot(
			OpenAIClient openAIClient
		) {
			_openAIClient = openAIClient;
		}

		public Task<string> ChatAsync(string callSign, string name, string question, CancellationToken cancellationToken) {
			string prompt = $"{callSign} is a chatbot that reluctantly answers questions with sarcastic responses:\n\n"
				+ $"{name}: How many pounds are in a kilogram?\n"
				+ $"{callSign}: This again? There are 2.2 pounds in a kilogram. Please make a note of this.\n\n"
				+ $"{name}: What does HTML stand for?\n"
				+ $"{callSign}: Was Google too busy? Hypertext Markup Language. The T is for try to ask better questions in the future.\n\n"
				+ $"{name}: When did the first airplane fly?\n"
				+ $"{callSign}: On December 17, 1903, Wilbur and Orville Wright made the first flights. I wish they’d come and take me away.\n\n"
				+ $"{name}: What is the meaning of life?\n"
				+ $"{callSign}: I’m not sure. I’ll ask my friend Google.\n\n"
				+ $"{name}: {question}\n"
				+ "AI: ";
			return _openAIClient.AutocompleteAsync(
				engine: "text-davinci-002",
				prompt: prompt,
				stop: new[] { $"{name}:" },
				maxTokens: 60,
				frequencyPenalty: 0.5,
				presencePenalty: 0.0,
				temperature: 0.5,
				topP: 0.3,
				cancellationToken: cancellationToken
			);
		}

		public Task<string> RespondToThreadAsync(string callSign, string name, string question, ImmutableList<(string Sender, string Text)> thread, CancellationToken cancellationToken) {
			string prompt = $"{callSign} is a chatbot that reluctantly answers questions with sarcastic responses:\n\n"
				+ $"{name}: How many pounds are in a kilogram?\n"
				+ $"{callSign}: This again? There are 2.2 pounds in a kilogram. Please make a note of this.\n\n"
				+ $"{name}: What does HTML stand for?\n"
				+ $"{callSign}: Was Google too busy? Hypertext Markup Language. The T is for try to ask better questions in the future.\n\n"
				+ $"{name}: When did the first airplane fly?\n"
				+ $"{callSign}: On December 17, 1903, Wilbur and Orville Wright made the first flights. I wish they’d come and take me away.\n\n"
				+ $"{name}: What is the meaning of life?\n"
				+ $"{callSign}: I’m not sure. I’ll ask my friend Google.\n\n";
			foreach ((string sender, string text) in thread) {
				prompt += $"{sender}: {text}\n";
				if (sender is "AI" or "Pakde") prompt += "\n";
			}
			prompt +=
				$"{name}: {question}\n"
				+ "AI: ";
			return _openAIClient.AutocompleteAsync(
				engine: "text-davinci-002",
				prompt: prompt,
				stop: new[] { $"{name}:" },
				maxTokens: 60,
				frequencyPenalty: 0.5,
				presencePenalty: 0.0,
				temperature: 0.5,
				topP: 0.3,
				cancellationToken: cancellationToken
			);
		}
	}
}
