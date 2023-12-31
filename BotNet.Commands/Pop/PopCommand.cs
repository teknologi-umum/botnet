using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands.Pop {
	public sealed record PopCommand : ICommand {
		public long ChatId { get; }

		private PopCommand(long chatId) {
			ChatId = chatId;
		}

		public static PopCommand FromSlashCommand(SlashCommand slashCommand) {
			if (slashCommand.Command != "/pop") {
				throw new ArgumentException("Command must be /pop.", nameof(slashCommand));
			}

			return new(
				chatId: slashCommand.ChatId
			);
		}
	}
}
