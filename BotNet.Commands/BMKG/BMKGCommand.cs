using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands.BMKG {
	public sealed record BMKGCommand : ICommand {
		public long ChatId { get; }
		public int CommandMessageId { get; }
		public long SenderId { get; }

		private BMKGCommand(
			long chatId,
			int commandMessageId,
			long senderId
		) {
			ChatId = chatId;
			CommandMessageId = commandMessageId;
			SenderId = senderId;
		}

		public static BMKGCommand FromSlashCommand(SlashCommand slashCommand) {
			if (slashCommand.Command != "/bmkg") {
				throw new ArgumentException("Command must be /bmkg.", nameof(slashCommand));
			}

			return new(
				chatId: slashCommand.ChatId,
				commandMessageId: slashCommand.MessageId,
				senderId: slashCommand.SenderId
			);
		}
	}
}
