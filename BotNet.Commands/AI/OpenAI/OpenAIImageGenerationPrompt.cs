﻿using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.SenderAggregate;

namespace BotNet.Commands.AI.OpenAI {
	public sealed record OpenAiImageGenerationPrompt : ICommand {
		public string CallSign { get; }
		public string Prompt { get; }
		public MessageId PromptMessageId { get; }
		public MessageId ResponseMessageId { get; }
		public ChatBase Chat { get; }
		public HumanSender Sender { get; }

		public OpenAiImageGenerationPrompt(
			string callSign,
			string prompt,
			MessageId promptMessageId,
			MessageId responseMessageId,
			ChatBase chat,
			HumanSender sender
		) {
			if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentException($"'{nameof(prompt)}' cannot be null or whitespace.", nameof(prompt));

			CallSign = callSign;
			Prompt = prompt;
			PromptMessageId = promptMessageId;
			ResponseMessageId = responseMessageId;
			Chat = chat;
			Sender = sender;
		}
	}
}
