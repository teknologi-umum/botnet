using BotNet.Commands.ChatAggregate;
using BotNet.Commands.SenderAggregate;

namespace BotNet.Commands.BotUpdate.Message {
	public abstract record MessageBase {
		public MessageId MessageId { get; private set; }
		public ChatBase Chat { get; private set; }
		public virtual SenderBase Sender { get; private set; }
		public string Text { get; private set; }
		public string? ImageFileId { get; private set; }
		public virtual MessageBase? ReplyToMessage { get; private set; }

		protected MessageBase(
			MessageId messageId,
			ChatBase chat,
			SenderBase sender,
			string text,
			string? imageFileId,
			MessageBase? replyToMessage
		) {
			MessageId = messageId;
			Chat = chat;
			Sender = sender;
			Text = text;
			ImageFileId = imageFileId;
			ReplyToMessage = replyToMessage;
		}
	}

	public abstract record HumanMessageBase : MessageBase {
		public override HumanSender Sender => (HumanSender)base.Sender;

		protected HumanMessageBase(
			MessageId messageId,
			ChatBase chat,
			HumanSender sender,
			string text,
			string? imageFileId,
			MessageBase? replyToMessage
		) : base(
			messageId: messageId,
			chat: chat,
			sender: sender,
			text: text,
			imageFileId: imageFileId,
			replyToMessage: replyToMessage
		) { }
	}

	public abstract record BotMessageBase : MessageBase {
		public override BotSender Sender => (BotSender)base.Sender;

		protected BotMessageBase(
			MessageId messageId,
			ChatBase chat,
			BotSender sender,
			string text,
			string? imageFileId,
			MessageBase? replyToMessage
		) : base(
			messageId: messageId,
			chat: chat,
			sender: sender,
			text: text,
			imageFileId: imageFileId,
			replyToMessage: replyToMessage
		) { }
	}
}
