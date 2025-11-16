using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Common;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Pick {
	public sealed record PickCommand : ICommand {
		public string[] Options { get; }
		public ChatBase Chat { get; }
		public MessageId MessageId { get; }
		public SenderBase Sender { get; }

		private PickCommand(
			string[] options,
			ChatBase chat,
			MessageId messageId,
			SenderBase sender
		) {
			Options = options;
			Chat = chat;
			MessageId = messageId;
			Sender = sender;
		}

		public static PickCommand FromSlashCommand(SlashCommand slashCommand) {
			string text = slashCommand.Text.Trim();

			// Validate that options are provided
			if (string.IsNullOrWhiteSpace(text)) {
				throw new UsageException(
					message: "Please provide options to pick from. Usage: /pick pizza sushi burger",
					parseMode: ParseMode.Html,
					commandMessageId: slashCommand.MessageId
				);
			}

			// Split by whitespace and filter out empty entries
			string[] options = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			if (options.Length < 2) {
				throw new UsageException(
					message: "Please provide at least 2 options to pick from.",
					parseMode: ParseMode.Html,
					commandMessageId: slashCommand.MessageId
				);
			}

			return new PickCommand(
				options: options,
				chat: slashCommand.Chat,
				messageId: slashCommand.MessageId,
				sender: slashCommand.Sender
			);
		}
	}
}
