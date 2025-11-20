using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Common;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Plot {
	public sealed record PlotCommand : ICommand {
		public string Expression { get; }
		public MessageId CommandMessageId { get; }
		public ChatBase Chat { get; }
		public HumanSender Sender { get; }

		private PlotCommand(
			string expression,
			MessageId commandMessageId,
			ChatBase chat,
			HumanSender sender
		) {
			Expression = expression;
			CommandMessageId = commandMessageId;
			Chat = chat;
			Sender = sender;
		}

		public static PlotCommand FromSlashCommand(SlashCommand slashCommand) {
			// Must be /plot
			if (slashCommand.Command != "/plot") {
				throw new ArgumentException("Command must be /plot.", nameof(slashCommand));
			}

			// Expression must be non-empty
			if (string.IsNullOrWhiteSpace(slashCommand.Text)) {
				throw new UsageException(
					message: "Expression tidak boleh kosong\\. Untuk membuat plot, silakan ketik `/plot` diikuti dengan ekspresi matematika\\.\n\nContoh: `/plot x + y = 1`",
					parseMode: ParseMode.MarkdownV2,
					commandMessageId: slashCommand.MessageId
				);
			}

			return new(
				expression: slashCommand.Text,
				commandMessageId: slashCommand.MessageId,
				chat: slashCommand.Chat,
				sender: slashCommand.Sender
			);
		}
	}
}
