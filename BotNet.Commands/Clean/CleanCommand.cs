using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands.Clean {
	public sealed record CleanCommand : ICommand {
		public SlashCommand Command { get; }

		private CleanCommand(SlashCommand command) {
			Command = command;
		}

		public static CleanCommand FromSlashCommand(SlashCommand command) {
			return new(command: command);
		}
	}
}
