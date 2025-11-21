using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Common;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Mermaid {
	public sealed record MermaidCommand : ICommand {
		public string MermaidCode { get; }
		public MessageId CommandMessageId { get; }
		public ChatBase Chat { get; }
		public HumanSender Sender { get; }

		private MermaidCommand(
			string mermaidCode,
			MessageId commandMessageId,
			ChatBase chat,
			HumanSender sender
		) {
			MermaidCode = mermaidCode;
			CommandMessageId = commandMessageId;
			Chat = chat;
			Sender = sender;
		}

		public static MermaidCommand FromSlashCommand(SlashCommand slashCommand) {
			// Must be /mermaid
			if (slashCommand.Command != "/mermaid") {
				throw new ArgumentException("Command must be /mermaid.", nameof(slashCommand));
			}

			// Code must be non-empty
			if (string.IsNullOrWhiteSpace(slashCommand.Text)) {
				throw new UsageException(
					message: "Kode Mermaid tidak boleh kosong\\. Untuk membuat diagram, silakan ketik `/mermaid` diikuti dengan kode Mermaid\\.\n\nContoh: `/mermaid graph TD; A-->B;`",
					parseMode: ParseMode.MarkdownV2,
					commandMessageId: slashCommand.MessageId
				);
			}

			return new(
				mermaidCode: slashCommand.Text,
				commandMessageId: slashCommand.MessageId,
				chat: slashCommand.Chat,
				sender: slashCommand.Sender
			);
		}

		public static MermaidCommand FromCodeBlock(
			string mermaidCode,
			MessageId messageId,
			ChatBase chat,
			HumanSender sender
		) {
			return new(
				mermaidCode: mermaidCode,
				commandMessageId: messageId,
				chat: chat,
				sender: sender
			);
		}
	}
}
