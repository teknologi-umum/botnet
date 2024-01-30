using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Common;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Art {
	public sealed record ArtCommand : ICommand {
		public string Prompt { get; }
		public MessageId PromptMessageId { get; }
		public ChatBase Chat { get; }
		public HumanSender Sender { get; }

		private ArtCommand(
			string prompt,
			MessageId promptMessageId,
			ChatBase chat,
			HumanSender sender
		) {
			Prompt = prompt;
			PromptMessageId = promptMessageId;
			Chat = chat;
			Sender = sender;
		}

		public static ArtCommand FromSlashCommand(SlashCommand slashCommand) {
			// Must be /art
			if (slashCommand.Command != "/art") {
				throw new ArgumentException("Command must be /art.", nameof(slashCommand));
			}

			// Prompt must be non-empty
			if (string.IsNullOrWhiteSpace(slashCommand.Text)) {
				throw new UsageException(
					message: "Prompt tidak boleh kosong\\. Untuk menghasilkan gambar, silakan ketik `/art` diikuti prompt\\.",
					parseMode: ParseMode.MarkdownV2,
					commandMessageId: slashCommand.MessageId
				);
			}

			return new(
				prompt: slashCommand.Text,
				promptMessageId: slashCommand.MessageId,
				chat: slashCommand.Chat,
				sender: slashCommand.Sender
			);
		}
	}
}
