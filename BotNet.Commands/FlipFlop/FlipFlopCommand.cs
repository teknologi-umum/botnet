﻿using BotNet.Commands.Common;
using BotNet.Commands.Telegram;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.FlipFlop {
	public sealed record FlipFlopCommand : ICommand {
		public string Command { get; }
		public long ChatId { get; }
		public int ImageMessageId { get; }
		public string ImageFileId { get; }

		private FlipFlopCommand(
			string command,
			long chatId,
			int imageMessageId,
			string imageFileId
		) {
			Command = command;
			ChatId = chatId;
			ImageMessageId = imageMessageId;
			ImageFileId = imageFileId;
		}

		public static FlipFlopCommand FromSlashCommand(SlashCommand slashCommand, MessageBase? repliedToMessage) {
			string commandName = slashCommand.Command switch {
				"/flip" => "flip",
				"/flop" => "flop",
				"/flep" => "flep",
				"/flap" => "flap",
				_ => throw new ArgumentException("Command must be /flip, /flop, /flep, or flap.", nameof(slashCommand))
			};

			// Must reply to a message
			if (slashCommand.ReplyToMessageId == null || repliedToMessage == null) {
				throw new UsageException(
					message: $"Apa yang mau di{commandName}? Untuk nge{commandName} gambar, reply `{slashCommand.Command}` ke pesan yang ada gambarnya\\.",
					parseMode: ParseMode.MarkdownV2,
					commandMessageId: slashCommand.MessageId
				);
			}

			// Must reply to repliedToMessage
			if (slashCommand.ReplyToMessageId != repliedToMessage.MessageId) {
				throw new ArgumentException("Reply to message ID must match replied to message ID.", nameof(repliedToMessage));
			}

			// Must reply to a message with a photo or sticker
			if (repliedToMessage.ImageFileId == null) {
				throw new UsageException(
					message: $"Pesan ini tidak ada gambarnya\\. Untuk nge{commandName} gambar, reply `{slashCommand.Command}` ke pesan yang ada gambarnya\\.",
					parseMode: ParseMode.MarkdownV2,
					commandMessageId: repliedToMessage.MessageId
				);
			}

			return new(
				command: slashCommand.Command,
				chatId: slashCommand.ChatId,
				imageMessageId: repliedToMessage.MessageId,
				imageFileId: repliedToMessage.ImageFileId
			);
		}
	}
}
