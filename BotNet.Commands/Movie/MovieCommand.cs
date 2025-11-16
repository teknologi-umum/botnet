using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Common;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Movie {
	public sealed record MovieCommand : ICommand {
		public string Title { get; }
		public ChatBase Chat { get; }
		public MessageId MessageId { get; }
		public SenderBase Sender { get; }

		private MovieCommand(
			string title,
			ChatBase chat,
			MessageId messageId,
			SenderBase sender
		) {
			Title = title;
			Chat = chat;
			MessageId = messageId;
			Sender = sender;
		}

		public static MovieCommand FromSlashCommand(SlashCommand slashCommand) {
			string title = slashCommand.Text.Trim();

			if (string.IsNullOrWhiteSpace(title)) {
				throw new UsageException(
					message: "Please provide a movie or TV series title. Usage: /movie Inception",
					parseMode: ParseMode.Html,
					commandMessageId: slashCommand.MessageId
				);
			}

			return new MovieCommand(
				title: title,
				chat: slashCommand.Chat,
				messageId: slashCommand.MessageId,
				sender: slashCommand.Sender
			);
		}
	}
}
