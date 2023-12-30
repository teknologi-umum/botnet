using System.Diagnostics.CodeAnalysis;
using BotNet.Commands.CommandPrioritization;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.BotUpdate.Message {
	public sealed record SlashCommand : MessageBase, ICommand {
		public string Command { get; }

		private SlashCommand(
			int messageId,
			long chatId,
			long senderId,
			string senderName,
			CommandPriority commandPriority,
			string text,
			string? imageFileId,
			int? replyToMessageId,
			MessageBase? replyToMessage,
			string command
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
			ArgumentNullException.ThrowIfNull(command);
			if (!command.StartsWith('/')) throw new ArgumentException("Command must start with a slash.", nameof(command));
			if (command.Length < 2) throw new ArgumentException("Command must have a name.", nameof(command));

			Command = command;
		}

		public static bool TryCreate(
			Telegram.Bot.Types.Message message,
			CommandPriority commandPriority,
			[NotNullWhen(true)] out SlashCommand? slashCommand
		) {
			// Message must start with a slash command
			if (message.Entities?.FirstOrDefault() is not {
				Type: MessageEntityType.BotCommand,
				Offset: 0,
				Length: int commandLength and > 1
			}) {
				slashCommand = null;
				return false;
			}

			// Sender must be a user
			if (message.From is not {
				IsBot: false,
				Id: long senderId,
				FirstName: string senderFirstName,
				LastName: var senderLastName
			}) {
				slashCommand = null;
				return false;
			}

			string senderFullName = senderLastName is null
				? senderFirstName
				: $"{senderFirstName} {senderLastName}";

			// Message must have text or a caption
			if ((message.Text ?? message.Caption) is not { } text
				|| text.Length < commandLength) {
				slashCommand = null;
				return false;
			}

			slashCommand = new(
				messageId: message.MessageId,
				chatId: message.Chat.Id,
				senderId: senderId,
				senderName: senderFullName,
				commandPriority: commandPriority,
				text: text[commandLength..].Trim(),
				imageFileId: message.Photo?.LastOrDefault()?.FileId,
				replyToMessageId: message.ReplyToMessage?.MessageId,
				replyToMessage: message.ReplyToMessage is null
					? null
					: NormalMessage.FromMessage(message.ReplyToMessage),
				command: text[..commandLength]
			);
			return true;
		}
	}
}
