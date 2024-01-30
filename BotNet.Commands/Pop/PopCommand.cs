using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;

namespace BotNet.Commands.Pop {
	public sealed record PopCommand : ICommand {
		public ChatBase Chat { get; }

		private PopCommand(ChatBase chat) {
			Chat = chat;
		}

		public static PopCommand FromSlashCommand(SlashCommand slashCommand) {
			if (slashCommand.Command != "/pop") {
				throw new ArgumentException("Command must be /pop.", nameof(slashCommand));
			}

			return new(
				chat: slashCommand.Chat
			);
		}
	}
}
