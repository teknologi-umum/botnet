using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Common;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Clean {
	public sealed record CleanCommand : ICommand {
		public string? TextToClean { get; }
		public string? ReplyToMessageText { get; }
		public MessageId CommandMessageId { get; }
		public MessageId? ReplyToMessageId { get; }
		public ChatBase Chat { get; }
		public HumanSender Sender { get; }

		private CleanCommand(
			string? textToClean,
			string? replyToMessageText,
			MessageId commandMessageId,
			MessageId? replyToMessageId,
			ChatBase chat,
			HumanSender sender
		) {
			TextToClean = textToClean;
			ReplyToMessageText = replyToMessageText;
			CommandMessageId = commandMessageId;
			ReplyToMessageId = replyToMessageId;
			Chat = chat;
			Sender = sender;
		}

		public static CleanCommand FromSlashCommand(SlashCommand slashCommand) {
			// Must be /clean
			if (slashCommand.Command != "/clean") {
				throw new ArgumentException("Command must be /clean.", nameof(slashCommand));
			}

			string? textToClean = null;
			string? replyToMessageText = null;
			MessageId? replyToMessageId = null;

			// Check if there's a command argument
			if (!string.IsNullOrWhiteSpace(slashCommand.Text)) {
				textToClean = slashCommand.Text.Trim();
			}
			// Otherwise check if replying to a message
			else if (slashCommand.ReplyToMessage?.Text is { } repliedToMessage) {
				replyToMessageText = repliedToMessage;
				replyToMessageId = slashCommand.ReplyToMessage.MessageId;
			}
			else {
				throw new UsageException(
					message: "<code>Tidak ada teks untuk dibersihkan. Balas pesan yang berisi link atau kirim link setelah perintah /clean.</code>",
					parseMode: ParseMode.Html,
					commandMessageId: slashCommand.MessageId
				);
			}

			return new(
				textToClean: textToClean,
				replyToMessageText: replyToMessageText,
				commandMessageId: slashCommand.MessageId,
				replyToMessageId: replyToMessageId,
				chat: slashCommand.Chat,
				sender: slashCommand.Sender
			);
		}
	}
}
