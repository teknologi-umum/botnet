using BotNet.Commands.ChatAggregate;
using BotNet.Commands.CommandPrioritization;
using BotNet.Commands.SenderAggregate;

namespace BotNet.Commands.BotUpdate.Message {
	public sealed record NormalMessage : MessageBase {
		private NormalMessage(
			MessageId messageId,
			ChatBase chat,
			SenderBase sender,
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

		public static NormalMessage FromMessage(Telegram.Bot.Types.Message message, CommandPriorityCategorizer commandPriorityCategorizer) {
			// Chat must be private or group
			if (!ChatBase.TryCreate(message.Chat, commandPriorityCategorizer, out ChatBase? chat)) {
				throw new ArgumentException("Chat must be private or group.", nameof(message));
			}

			// Sender must not be null
			if (message.From is not { } from) {
				throw new ArgumentException("Message must have a sender.", nameof(message));
			}

			return new(
				messageId: new(message.MessageId),
				chat: chat,
				sender: HumanSender.TryCreate(from, commandPriorityCategorizer, out HumanSender? humanSender)
					? humanSender
					: BotSender.TryCreate(from, out BotSender? botSender)
						? botSender
						: throw new ArgumentException("Unknown sender type.", nameof(message)),
				text: message.Text ?? "",
				imageFileId: message.Photo?.LastOrDefault()?.FileId ?? message.Sticker?.FileId,
				replyToMessage: message.ReplyToMessage is null
					? null
					: NormalMessage.FromMessage(message.ReplyToMessage, commandPriorityCategorizer)
			);
		}
	}
}
