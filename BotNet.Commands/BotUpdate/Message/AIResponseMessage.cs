using BotNet.Commands.CommandPrioritization;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.BotUpdate.Message {
	public sealed record AIResponseMessage : MessageBase {
		public string CallSign { get; }

		private AIResponseMessage(
			int messageId,
			long chatId,
			ChatType chatType,
			string? chatTitle,
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
			chatType: chatType,
			chatTitle: chatTitle,
			senderId: senderId,
			senderName: senderName,
			commandPriority: CommandPriority.Void,
			text: text,
			imageFileId: imageFileId,
			replyToMessageId: replyToMessageId,
			replyToMessage: replyToMessage
		) {
			CallSign = callSign;
		}

		public static AIResponseMessage FromMessage(
			Telegram.Bot.Types.Message message,
			int replyToMessageId,
			string callSign
		) {
			return new(
				messageId: message.MessageId,
				chatId: message.Chat.Id,
				chatType: message.Chat.Type,
				chatTitle: message.Chat.Title,
				senderId: message.From!.Id,
				senderName: callSign,
				text: message.Text ?? message.Caption ?? "",
				imageFileId: message.Photo?.FirstOrDefault()?.FileId,
				replyToMessageId: replyToMessageId,
				replyToMessage: null,
				callSign: callSign
			);
		}
	}
}
