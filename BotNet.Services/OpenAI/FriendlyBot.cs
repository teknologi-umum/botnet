using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.OpenAI.Models;

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
			foreach ((string sender, string text) in thread) {
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

		public Task<string> ChatAsync(string message, CancellationToken cancellationToken) {
			List<ChatMessage> messages = new() {
				new("system", "The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly."),
				new("user", message)
			};

			return _openAIClient.ChatAsync(
				model: "gpt-3.5-turbo",
				messages: messages,
				maxTokens: 2048,
				cancellationToken: cancellationToken
			);
		}

		public Task<string> ChatAsync(string message, ImmutableList<(string Sender, string Text)> thread, CancellationToken cancellationToken) {
			List<ChatMessage> messages = new() {
				new("system", "The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly."),

				from tuple in thread
				select new ChatMessage(
					Role: tuple.Sender switch {
						"AI" => "assistant",
						_ => "user"
					},
					Content: tuple.Text
				),

				new("user", message)
			};

			return _openAIClient.ChatAsync(
				model: "gpt-3.5-turbo",
				messages: messages,
				maxTokens: 2048,
				cancellationToken: cancellationToken
			);
		}
	}
}
