﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.OpenAI.Models;

namespace BotNet.Services.OpenAI.Skills {
	public sealed class FriendlyBot(
		OpenAIClient openAIClient,
		OpenAIStreamingClient openAIStreamingClient
	) {
		private readonly OpenAIClient _openAIClient = openAIClient;
		private readonly OpenAIStreamingClient _openAIStreamingClient = openAIStreamingClient;

		public Task<string> ChatAsync(string callSign, string name, string question, CancellationToken cancellationToken) {
			string prompt = $"The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly.\n\n"
				+ $"{name}: Hello, how are you?\n"
				+ $"{callSign}: I am an AI created by TEKNUM. How can I help you today?\n\n"
				+ $"{name}: {question}\n"
				+ $"{callSign}: ";
			return _openAIClient.AutocompleteAsync(
				engine: "text-davinci-003",
				prompt: prompt,
				stop: [$"{name}:"],
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
				stop: [$"{name}:"],
				maxTokens: 128,
				frequencyPenalty: 0.5,
				presencePenalty: 0.6,
				temperature: 0.9,
				topP: 1.0,
				cancellationToken: cancellationToken
			);
		}

		public Task<string> ChatAsync(string message, CancellationToken cancellationToken) {
			List<ChatMessage> messages = [
				ChatMessage.FromText("system", "The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly."),
				ChatMessage.FromText("user", message)
			];

			return _openAIClient.ChatAsync(
				model: "gpt-4-1106-preview",
				messages: messages,
				maxTokens: 512,
				cancellationToken: cancellationToken
			);
		}

		public async Task StreamChatAsync(string message, long chatId, int replyToMessageId) {
			List<ChatMessage> messages = [
				ChatMessage.FromText("system", "The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly."),
				ChatMessage.FromText("user", message)
			];

			await _openAIStreamingClient.StreamChatAsync(
				model: "gpt-4-1106-preview",
				messages: messages,
				maxTokens: 512,
				callSign: "AI",
				chatId: chatId,
				replyToMessageId: replyToMessageId
			);
		}

		public Task<string> ChatAsync(string message, ImmutableList<(string Sender, string? Text, string? ImageBase64)> thread, CancellationToken cancellationToken) {
			List<ChatMessage> messages = new() {
				ChatMessage.FromText("system", "The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly."),

				from tuple in thread
				let role = tuple.Sender switch {
					"AI" => "assistant",
					_ => "user"
				}
				select tuple switch {
					{ Text: { } text, ImageBase64: null } => ChatMessage.FromText(role, text),
					{ Text: null, ImageBase64: { } imageBase64 } => ChatMessage.FromImageBase64(role, imageBase64),
					{ Text: { } text, ImageBase64: { } imageBase64 } => ChatMessage.FromTextWithImageBase64(role, text, imageBase64),
					_ => ChatMessage.FromText(role, "")
				},

				ChatMessage.FromText("user", message)
			};

			return _openAIClient.ChatAsync(
				model: "gpt-4-1106-preview",
				messages: messages,
				maxTokens: 512,
				cancellationToken: cancellationToken
			);
		}

		public async Task StreamChatAsync(string message, ImmutableList<(string Sender, string? Text, string? ImageBase64)> thread, long chatId, int replyToMessageId) {
			List<ChatMessage> messages = new() {
				ChatMessage.FromText("system", "The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly."),

				from tuple in thread
				let role = tuple.Sender switch {
					"AI" => "assistant",
					_ => "user"
				}
				select tuple switch {
					{ Text: { } text, ImageBase64: null } => ChatMessage.FromText(role, text),
					{ Text: null, ImageBase64: { } imageBase64 } => ChatMessage.FromImageBase64(role, imageBase64),
					{ Text: { } text, ImageBase64: { } imageBase64 } => ChatMessage.FromTextWithImageBase64(role, text, imageBase64),
					_ => ChatMessage.FromText(role, "")
				},

				ChatMessage.FromText("user", message)
			};

			await _openAIStreamingClient.StreamChatAsync(
				model: "gpt-4-1106-preview",
				messages: messages,
				maxTokens: 512,
				callSign: "AI",
				chatId: chatId,
				replyToMessageId: replyToMessageId
			);
		}
	}
}
