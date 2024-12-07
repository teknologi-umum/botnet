using System.Diagnostics.CodeAnalysis;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.CommandPrioritization;
using BotNet.Commands.SenderAggregate;

namespace BotNet.Commands.BotUpdate.Message {
	public sealed record AiFollowUpMessage : HumanMessageBase, ICommand {
		public override AiResponseMessage ReplyToMessage => (AiResponseMessage)base.ReplyToMessage!;
		public string CallSign => ReplyToMessage.CallSign;

		private AiFollowUpMessage(
			MessageId messageId,
			ChatBase chat,
			HumanSender sender,
			string text,
			string? imageFileId,
			AiResponseMessage replyToMessage
		) : base(
			messageId: messageId,
			chat: chat,
			sender: sender,
			text: text,
			imageFileId: imageFileId,
			replyToMessage: replyToMessage
		) { }

		public static bool TryCreate(
			Telegram.Bot.Types.Message message,
			IEnumerable<MessageBase> thread,
			CommandPriorityCategorizer commandPriorityCategorizer,
			[NotNullWhen(true)] out AiFollowUpMessage? aiFollowUpMessage
		) {
			// Chat must be private or group
			if (!ChatBase.TryCreate(message.Chat, commandPriorityCategorizer, out ChatBase? chat)) {
				aiFollowUpMessage = null;
				return false;
			}

			// Sender must be a user
			if (message.From is not { } from ||
			    !HumanSender.TryCreate(from, commandPriorityCategorizer, out HumanSender? sender)) {
				aiFollowUpMessage = null;
				return false;
			}

			// Message must contain text or caption
			if ((message.Text ?? message.Caption) is not { } text) {
				aiFollowUpMessage = null;
				return false;
			}

			// Must reply to AI response message
			if (thread.FirstOrDefault() is not AiResponseMessage { CallSign: string } aiResponseMessage ||
			    aiResponseMessage.MessageId != message.ReplyToMessage?.MessageId) {
				aiFollowUpMessage = null;
				return false;
			}

			// Sender must be a user
			if (message.From is not {
				    IsBot: false,
			    }) {
				aiFollowUpMessage = null;
				return false;
			}

			aiFollowUpMessage = new(
				messageId: new(message.MessageId),
				chat: chat,
				sender: sender,
				text: text,
				imageFileId: message.Photo?.FirstOrDefault()
					?.FileId,
				replyToMessage: aiResponseMessage
			);
			return true;
		}
	}
}
