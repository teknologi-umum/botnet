﻿using BotNet.Commands.CommandPrioritization;

namespace BotNet.Commands.BotUpdate.Message {
	public sealed record NormalMessage : MessageBase {
		private NormalMessage(
			int messageId,
			long chatId,
			long senderId,
			string senderName,
			string text,
			string? imageFileId,
			int? replyToMessageId,
			MessageBase? replyToMessage
		) : base(
			messageId: messageId,
			chatId: chatId,
			senderId: senderId,
			senderName: senderName,
			commandPriority: CommandPriority.Void,
			text: text,
			imageFileId: imageFileId,
			replyToMessageId: replyToMessageId,
			replyToMessage: replyToMessage
		) { }

		public static NormalMessage FromMessage(Telegram.Bot.Types.Message message) {
			// Sender must not be null
			if (message.From is not {
				Id: long senderId,
				FirstName: string senderFirstName,
				LastName: var senderLastName
			}) {
				throw new ArgumentException("Message must have a sender.", nameof(message));
			}

			string senderFullName = message.From is null
				? senderFirstName
				: $"{senderFirstName} {senderLastName}";

			return new(
				messageId: message.MessageId,
				chatId: message.Chat.Id,
				senderId: senderId,
				senderName: senderFullName,
				text: message.Text ?? "",
				imageFileId: message.Photo?.LastOrDefault()?.FileId ?? message.Sticker?.FileId,
				replyToMessageId: message.ReplyToMessage?.MessageId,
				replyToMessage: message.ReplyToMessage is null
					? null
					: NormalMessage.FromMessage(message.ReplyToMessage)
			);
		}
	}
}