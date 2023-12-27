namespace BotNet.Commands.BotUpdate.Message {
	public sealed record AIResponseMessage : MessageBase {
		public string CallSign { get; }

		private AIResponseMessage(
			int messageId,
			long chatId,
			long senderId,
			string senderName,
			string text,
			string? imageFileId,
			int? replyToMessageId,
			MessageBase? replyToMessage,
			string callSign
		) : base(
			messageId: messageId,
			chatId: chatId,
			senderId: senderId,
			senderName: senderName,
			text: text,
			imageFileId: imageFileId,
			replyToMessageId: replyToMessageId,
			replyToMessage: replyToMessage
		) {
			CallSign = callSign;
		}

		public static AIResponseMessage FromMessage(
			Telegram.Bot.Types.Message message,
			string callSign
		) {
			// Message must be a reply
			if (message.ReplyToMessage is not { } replyToMessage) {
				throw new ArgumentException("Message must be a reply.", nameof(message));
			}

			return new(
				messageId: message.MessageId,
				chatId: message.Chat.Id,
				senderId: message.From!.Id,
				senderName: callSign,
				text: message.Text ?? message.Caption ?? "",
				imageFileId: message.Photo?.FirstOrDefault()?.FileId,
				replyToMessageId: replyToMessage.MessageId,
				replyToMessage: null,
				callSign: callSign
			);
		}
	}
}
