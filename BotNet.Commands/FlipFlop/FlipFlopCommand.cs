using BotNet.Commands.Common;
using Telegram.Bot.Types.Enums;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;

namespace BotNet.Commands.FlipFlop {
	public sealed record FlipFlopCommand : ICommand {
		public string Command { get; }
		public string ImageFileId { get; }
		public MessageId ImageMessageId { get; }
		public ChatBase Chat { get; }

		private FlipFlopCommand(
			string command,
			string imageFileId,
			MessageId imageMessageId,
			ChatBase chat
		) {
			Command = command;
			ImageFileId = imageFileId;
			ImageMessageId = imageMessageId;
			Chat = chat;
		}

		public static FlipFlopCommand FromSlashCommand(SlashCommand slashCommand) {
			string commandName = slashCommand.Command switch {
				"/flip" => "flip",
				"/flop" => "flop",
				"/flep" => "flep",
				"/flap" => "flap",
				_ => throw new ArgumentException("Command must be /flip, /flop, /flep, or flap.", nameof(slashCommand))
			};

			// Must reply to a message
			if (slashCommand.ReplyToMessage == null) {
				throw new UsageException(
					message: $"Apa yang mau di{commandName}? Untuk nge{commandName} gambar, reply `{slashCommand.Command}` ke pesan yang ada gambarnya\\.",
					parseMode: ParseMode.MarkdownV2,
					commandMessageId: slashCommand.MessageId
				);
			}

			// Must reply to a message with a photo or sticker
			if (slashCommand.ReplyToMessage.ImageFileId == null) {
				throw new UsageException(
					message: $"Pesan ini tidak ada gambarnya\\. Untuk nge{commandName} gambar, reply `{slashCommand.Command}` ke pesan yang ada gambarnya\\.",
					parseMode: ParseMode.MarkdownV2,
					commandMessageId: slashCommand.ReplyToMessage.MessageId
				);
			}

			return new(
				command: slashCommand.Command,
				imageFileId: slashCommand.ReplyToMessage.ImageFileId,
				imageMessageId: slashCommand.ReplyToMessage.MessageId,
				chat: slashCommand.Chat
			);
		}
	}
}
