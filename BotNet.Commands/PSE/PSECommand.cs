using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Common;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.PSE {
	public sealed record PSECommand : ICommand {
		public string Keyword { get; }
		public MessageId CommandMessageId { get; }
		public ChatBase Chat { get; }
		public HumanSender Sender { get; }

		public PSECommand(
			string keyword,
			MessageId commandMessageId,
			ChatBase chat,
			HumanSender sender
		) {
			Keyword = keyword;
			CommandMessageId = commandMessageId;
			Chat = chat;
			Sender = sender;
		}

		public static PSECommand FromSlashCommand(SlashCommand slashCommand) {
			// Must be /pse
			if (slashCommand.Command != "/pse") {
				throw new ArgumentException("Command must be /pse.", nameof(slashCommand));
			}

			// Keyword must be non-empty
			if (string.IsNullOrWhiteSpace(slashCommand.Text)) {
				throw new UsageException(
					message: "Untuk mencari sistem elektronik, silakan ketik `/pse` diikuti keyword\\.",
					parseMode: ParseMode.MarkdownV2,
					commandMessageId: slashCommand.MessageId
				);
			}

			return new(
				keyword: slashCommand.Text,
				commandMessageId: slashCommand.MessageId,
				chat: slashCommand.Chat,
				sender: slashCommand.Sender
			);
		}
	}
}
