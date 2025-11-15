using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands.No {
	public sealed record NoCommand : ICommand {
		public SlashCommand Command { get; }

		private NoCommand(SlashCommand command) {
			Command = command;
		}

		public static NoCommand FromSlashCommand(SlashCommand command) {
			return new(command: command);
		}
	}
}
