using System.Diagnostics.CodeAnalysis;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.CommandPrioritization;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.BotUpdate.Message {
	public sealed record SlashCommand : HumanMessageBase, ICommand {
		public string Command { get; }
		public bool IsMentioned { get; }

		private SlashCommand(
			MessageId messageId,
			ChatBase chat,
			HumanSender sender,
			string text,
			string? imageFileId,
			MessageBase? replyToMessage,
			string command,
			bool isMentioned
		) : base(
			messageId: messageId,
			chat: chat,
			sender: sender,
			text: text,
			imageFileId: imageFileId,
			replyToMessage: replyToMessage
		) {
			ArgumentNullException.ThrowIfNull(command);
			if (!command.StartsWith('/')) throw new ArgumentException("Command must start with a slash.", nameof(command));
			if (command.Length < 2) throw new ArgumentException("Command must have a name.", nameof(command));

			Command = command;
			IsMentioned = isMentioned;
		}

		public static bool TryCreate(
			Telegram.Bot.Types.Message message,
			string botUsername,
			CommandPriorityCategorizer commandPriorityCategorizer,
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

			// Chat must be private or group
			if (!ChatBase.TryCreate(message.Chat, commandPriorityCategorizer, out ChatBase? chat)) {
				slashCommand = null;
				return false;
			}

			// Sender must be a user
			if (message.From is not { } from
				|| !HumanSender.TryCreate(from, commandPriorityCategorizer, out HumanSender? sender)) {
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
			if (commandText.IndexOf('@') is int ampersandPos
				&& ampersandPos != -1) {
				string targetUsername = commandText[(ampersandPos + 1)..];
				if (!StringComparer.OrdinalIgnoreCase.Equals(targetUsername, botUsername)) {
					slashCommand = null;
					return false;
				}

				// Simplify command text
				commandText = commandText[..ampersandPos];
			}

			slashCommand = new(
				messageId: new(message.MessageId),
				chat: chat,
				sender: sender,
				text: arg,
				imageFileId: message.Photo?.LastOrDefault()?.FileId,
				replyToMessage: message.ReplyToMessage is null
					? null
					: NormalMessage.FromMessage(message.ReplyToMessage, commandPriorityCategorizer),
				command: commandText,
				isMentioned: ampersandPos != -1
			);
			return true;
		}
	}
}
