using System.Diagnostics.CodeAnalysis;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.CommandPrioritization;
using BotNet.Commands.SenderAggregate;

namespace BotNet.Commands.BotUpdate.Message {
	public sealed record AIFollowUpMessage : HumanMessageBase, ICommand {
		public override AIResponseMessage ReplyToMessage => (AIResponseMessage)base.ReplyToMessage!;
		public string CallSign => ReplyToMessage.CallSign;

		public AIFollowUpMessage(
			MessageId messageId,
			ChatBase chat,
			HumanSender sender,
			string text,
			string? imageFileId,
			AIResponseMessage replyToMessage
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
			[NotNullWhen(true)] out AIFollowUpMessage? aiFollowUpMessage
		) {
			// Chat must be private or group
			if (!ChatBase.TryCreate(message.Chat, commandPriorityCategorizer, out ChatBase? chat)) {
				aiFollowUpMessage = null;
				return false;
			}

			// Sender must be a user
			if (message.From is not { } from
				|| !HumanSender.TryCreate(from, commandPriorityCategorizer, out HumanSender? sender)) {
				aiFollowUpMessage = null;
				return false;
			}

			// Message must contain text or caption
			if ((message.Text ?? message.Caption) is not { } text) {
				aiFollowUpMessage = null;
				return false;
			}

			// Must reply to AI response message
			if (thread.FirstOrDefault() is not AIResponseMessage { CallSign: string callSign } aiResponseMessage
				|| aiResponseMessage.MessageId != message.ReplyToMessage?.MessageId) {
				aiFollowUpMessage = null;
				return false;
			}

			// Sender must be a user
			if (message.From is not {
				IsBot: false,
				Id: long senderId,
				FirstName: string senderFirstName,
				LastName: var senderLastName
			}) {
				aiFollowUpMessage = null;
				return false;
			}

			string senderFullName = senderLastName is null
				? senderFirstName
				: $"{senderFirstName} {senderLastName}";

			aiFollowUpMessage = new(
				messageId: new(message.MessageId),
				chat: chat,
				sender: sender,
				text: text,
				imageFileId: message.Photo?.FirstOrDefault()?.FileId,
				replyToMessage: aiResponseMessage
			);
			return true;
		}
	}
}
