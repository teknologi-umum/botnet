using BotNet.Commands.CommandPrioritization;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.BotUpdate.Message {
	public abstract record MessageBase {
		public int MessageId { get; private set; }
		public long ChatId { get; private set; }
		public ChatType ChatType { get; private set; }
		public string? ChatTitle { get; private set; }
		public long SenderId { get; private set; }
		public string SenderName { get; private set; }
		public CommandPriority CommandPriority { get; private set; }
		public string Text { get; private set; }
		public string? ImageFileId { get; private set; }
		public int? ReplyToMessageId { get; private set; }
		public MessageBase? ReplyToMessage { get; private set; }

		protected MessageBase(
			int messageId,
			long chatId,
			ChatType chatType,
			string? chatTitle,
			long senderId,
			string senderName,
			CommandPriority commandPriority,
			string text,
			string? imageFileId,
			int? replyToMessageId,
			MessageBase? replyToMessage
		) {
			MessageId = messageId;
			ChatId = chatId;
			ChatType = chatType;
			ChatTitle = chatTitle;
			SenderId = senderId;
			SenderName = senderName;
			CommandPriority = commandPriority;
			Text = text;
			ImageFileId = imageFileId;
			ReplyToMessageId = replyToMessageId;
			ReplyToMessage = replyToMessage;
		}
	}
}
