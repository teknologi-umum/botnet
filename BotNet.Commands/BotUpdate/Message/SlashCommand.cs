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
			string botUsername,
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

			// Message must have text or a caption
			if ((message.Text ?? message.Caption) is not { } text
				|| text.Length < commandLength) {
				slashCommand = null;
				return false;
			}

			string commandText = text[..commandLength];
			string arg = text[commandLength..].Trim();

			// Command must be for this bot
			if (commandText.IndexOf('@') is int ampersandPos and not -1) {
				string targetUsername = commandText[(ampersandPos + 1)..];
				if (!StringComparer.OrdinalIgnoreCase.Equals(targetUsername, botUsername)) {
					slashCommand = null;
					return false;
				}

				// Simplify command text
				commandText = commandText[..ampersandPos];
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

			slashCommand = new(
				messageId: message.MessageId,
				chatId: message.Chat.Id,
				senderId: senderId,
				senderName: senderFullName,
				commandPriority: commandPriority,
				text: arg,
				imageFileId: message.Photo?.LastOrDefault()?.FileId,
				replyToMessageId: message.ReplyToMessage?.MessageId,
				replyToMessage: message.ReplyToMessage is null
					? null
					: NormalMessage.FromMessage(message.ReplyToMessage),
				command: commandText
			);
			return true;
		}
	}
}
