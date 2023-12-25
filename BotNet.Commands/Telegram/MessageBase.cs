using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Telegram {
	public abstract record MessageBase {
		public int MessageId { get; private set; }
		public long ChatId { get; private set; }
		public long SenderId { get; private set; }
		public string SenderName { get; private set; }
		public string Text { get; private set; }
		public string? ImageFileId { get; private set; }
		public int? ReplyToMessageId { get; private set; }
		public MessageBase? ReplyToMessage { get; private set; }

		protected MessageBase(
			int messageId,
			long chatId,
			long senderId,
			string senderName,
			string text,
			string? imageFileId,
			int? replyToMessageId,
			MessageBase? replyToMessage
		) {
			MessageId = messageId;
			ChatId = chatId;
			SenderId = senderId;
			SenderName = senderName;
			Text = text;
			ImageFileId = imageFileId;
			ReplyToMessageId = replyToMessageId;
			ReplyToMessage = replyToMessage;
		}

		public static MessageBase FromMessage(Message message) {
			// Handle slash command
			if (message.Entities?.FirstOrDefault() is {
				Type: MessageEntityType.BotCommand,
				Offset: 0,
				Length: > 1
			}) {
				if (!SlashCommand.TryCreate(
					message: message,
					slashCommand: out SlashCommand? slashCommand
				)) {
					throw new ArgumentException("Could not parse message into a slash command.", nameof(message));
				}

				return slashCommand;
			}

			// TODO: handle AI calls
			// TODO: handle AI thread replies

			return NormalMessage.FromMessage(message);
		}
	}
}
