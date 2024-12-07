using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BotNet.Services.OpenAI.Models;

namespace BotNet.Services.OpenAI.Skills {
	public sealed class VisionBot(
		OpenAiStreamingClient openAiStreamingClient
	) {
		public async Task StreamChatAsync(
			string message,
			string imageBase64,
			long chatId,
			int replyToMessageId
		) {
			List<ChatMessage> messages = new() {
				ChatMessage.FromText("system", "The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly."),
				ChatMessage.FromTextWithImageBase64("user", message, imageBase64)
			};

			await openAiStreamingClient.StreamChatAsync(
				model: "gpt-4-vision-preview",
				messages: messages,
				maxTokens: 512,
				callSign: "GPT",
				chatId: chatId,
				replyToMessageId: replyToMessageId
			);
		}

		public async Task StreamChatAsync(
			string message,
			string imageBase64,
			ImmutableList<(string Sender, string? Text, string? ImageBase64)> thread,
			long chatId,
			int replyToMessageId
		) {
			List<ChatMessage> messages = new() {
				ChatMessage.FromText("system", "The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly."),

				from tuple in thread
				let role = tuple.Sender switch {
					"GPT" => "assistant",
					_ => "user"
				}
				select tuple switch {
					{ Text: { } text, ImageBase64: null } => ChatMessage.FromText(role, text),
					{ Text: null, ImageBase64: { } imageBase64 } => ChatMessage.FromImageBase64(role, imageBase64),
					{ Text: { } text, ImageBase64: { } imageBase64 } => ChatMessage.FromTextWithImageBase64(role, text, imageBase64),
					_ => ChatMessage.FromText(role, "")
				},

				ChatMessage.FromTextWithImageBase64("user", message, imageBase64)
			};

			await openAiStreamingClient.StreamChatAsync(
				model: "gpt-4-vision-preview",
				messages: messages,
				maxTokens: 512,
				callSign: "GPT",
				chatId: chatId,
				replyToMessageId: replyToMessageId
			);
		}
	}
}
