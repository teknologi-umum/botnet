﻿using BotNet.Commands.ChatAggregate;
using BotNet.Commands.CommandPrioritization;
using BotNet.Commands.SenderAggregate;

namespace BotNet.Commands.BotUpdate.Message {
	public sealed record AiResponseMessage : MessageBase {
		public string CallSign { get; }

		private AiResponseMessage(
			MessageId messageId,
			ChatBase chat,
			BotSender sender,
			string text,
			string? imageFileId,
			HumanMessageBase? replyToMessage,
			string callSign
		) : base(
			messageId: messageId,
			chat: chat,
			sender: sender,
			text: text,
			imageFileId: imageFileId,
			replyToMessage: replyToMessage
		) {
			CallSign = callSign;
		}

		public static AiResponseMessage FromMessage(
			Telegram.Bot.Types.Message message,
			HumanMessageBase replyToMessage,
			string callSign,
			CommandPriorityCategorizer commandPriorityCategorizer
		) {
			// Chat must be private or group
			if (!ChatBase.TryCreate(message.Chat, commandPriorityCategorizer, out ChatBase? chat)) {
				throw new ArgumentException("Chat must be private or group.", nameof(message));
			}

			// Sender must be a bot
			if (message.From is not { } from
				|| !BotSender.TryCreate(from, out BotSender? sender)) {
				throw new ArgumentException("Sender must be bot.", nameof(message));
			}

			return new(
				messageId: new(message.MessageId),
				chat: chat,
				sender: sender,
				text: message.Text ?? message.Caption ?? "",
				imageFileId: message.Photo?.FirstOrDefault()?.FileId,
				replyToMessage: replyToMessage,
				callSign: callSign
			);
		}
	}
}
