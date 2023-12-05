using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.OpenAI.Models;

namespace BotNet.Services.OpenAI {
	public sealed class IntentDetector(
		OpenAIClient openAIClient
	) {
		private readonly OpenAIClient _openAIClient = openAIClient;

		public async Task<ChatIntent> DetectChatIntentAsync(
			string message,
			CancellationToken cancellationToken
		) {
			List<ChatMessage> messages = [
				ChatMessage.FromText("user", $$"""
				These are available intents that one might query when they provide a text prompt:

				    Question,
				    ImageGeneration

				Which intent is this query asking for? If none match, respond with Unknown.

				{{message}}

				Intent:

				""")
			];

			string answer = await _openAIClient.ChatAsync(
				model: "gpt-3.5-turbo",
				messages: messages,
				maxTokens: 128,
				cancellationToken: cancellationToken
			);

			return answer switch {
				"Question" => ChatIntent.Question,
				"ImageGeneration" => ChatIntent.ImageGeneration,
				"Unknown" => ChatIntent.Question,
				_ => ChatIntent.Question
			};
		}

		public async Task<ImagePromptIntent> DetectImagePromptIntentAsync(
			string message,
			CancellationToken cancellationToken
		) {
			List<ChatMessage> messages = [
				ChatMessage.FromText("user", $$"""
				These are available intents that one might query when they provide a prompt which contain an image:

				    Vision,
					ImageVariation

				Which intent is this query asking for? If none match, respond with Unknown.

				{{message}}

				Intent:

				""")
];

			string answer = await _openAIClient.ChatAsync(
				model: "gpt-3.5-turbo",
				messages: messages,
				maxTokens: 128,
				cancellationToken: cancellationToken
			);

			return answer switch {
				"Vision" => ImagePromptIntent.Vision,
				"ImageVariation" => ImagePromptIntent.ImageVariation,
				"Unknown" => ImagePromptIntent.Vision,
				_ => ImagePromptIntent.Vision
			};
		}
	}
}
