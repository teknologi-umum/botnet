using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using BotNet.Commands.CommandPrioritization;

namespace BotNet.Commands.BotUpdate.Message {
	public sealed record AICallCommand : MessageBase, ICommand {
		public static readonly ImmutableHashSet<string> CALL_SIGNS = [
			"AI",
			"Bot",
			"GPT",
			"Gemini",
			"Pakde"
		];

		public string CallSign { get; }

		private AICallCommand(
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
			[NotNullWhen(true)] out AICallCommand? aiCallCommand
		) {
			// Message must contain text or caption
			if ((message.Text ?? message.Caption) is not { } text) {
				aiCallCommand = null;
				return false;
			}

			// Message must start with call sign
			if (CALL_SIGNS.FirstOrDefault(callSign => text.StartsWith($"{callSign},", StringComparison.OrdinalIgnoreCase)) is not { } callSign) {
				aiCallCommand = null;
				return false;
			}

			// Sender must be a user
			if (message.From is not {
				IsBot: false,
				Id: long senderId,
				FirstName: string senderFirstName,
				LastName: var senderLastName
			}) {
				aiCallCommand = null;
				return false;
			}

			string senderFullName = senderLastName is null
				? senderFirstName
				: $"{senderFirstName} {senderLastName}";

			aiCallCommand = new(
				messageId: message.MessageId,
				chatId: message.Chat.Id,
				senderId: senderId,
				senderName: senderFullName,
				commandPriority: commandPriority,
				text: text[(callSign.Length + 1)..].Trim(),
				imageFileId: message.Photo?.LastOrDefault()?.FileId
					?? message.ReplyToMessage?.Sticker?.FileId,
				replyToMessageId: message.ReplyToMessage?.MessageId,
				replyToMessage: message.ReplyToMessage is null
					? null
					: NormalMessage.FromMessage(message.ReplyToMessage),
				callSign: callSign
			);
			return true;
		}
	}
}
