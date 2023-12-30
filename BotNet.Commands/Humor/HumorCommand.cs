using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands.Humor {
	public sealed record HumorCommand : ICommand {
		public long ChatId { get; }
		public int CommandMessageId { get; }

		private HumorCommand(
			long chatId,
			int commandMessageId
		) {
			ChatId = chatId;
			CommandMessageId = commandMessageId;
		}

		public static HumorCommand FromSlashCommand(SlashCommand slashCommand) {
			if (slashCommand.Command != "/humor") {
				throw new ArgumentException("Command must be /humor.", nameof(slashCommand));
			}

			return new(
				chatId: slashCommand.ChatId,
				commandMessageId: slashCommand.MessageId
			);
		}
	}
}
