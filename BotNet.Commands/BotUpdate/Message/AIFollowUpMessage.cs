using System.Diagnostics.CodeAnalysis;
using BotNet.Commands.CommandPrioritization;

namespace BotNet.Commands.BotUpdate.Message {
	public sealed record AIFollowUpMessage : MessageBase, ICommand {
		public string CallSign { get; }

		public AIFollowUpMessage(
			int messageId,
			long chatId,
			long senderId,
			string senderName,
			CommandPriority commandPriority,
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
			commandPriority: commandPriority,
			text: text,
			imageFileId: imageFileId,
			replyToMessageId: replyToMessageId,
			replyToMessage: replyToMessage
		) {
			CallSign = callSign;
		}

		public static bool TryCreate(
			Telegram.Bot.Types.Message message,
			CommandPriority commandPriority,
			IEnumerable<MessageBase> thread,
			[NotNullWhen(true)] out AIFollowUpMessage? aiFollowUpMessage
		) {
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
				messageId: message.MessageId,
				chatId: message.Chat.Id,
				senderId: senderId,
				senderName: senderFullName,
				commandPriority: commandPriority,
				text: text,
				imageFileId: message.Photo?.FirstOrDefault()?.FileId,
				replyToMessageId: message.ReplyToMessage?.MessageId,
				replyToMessage: aiResponseMessage,
				callSign: callSign
			);
			return true;
		}
	}
}
