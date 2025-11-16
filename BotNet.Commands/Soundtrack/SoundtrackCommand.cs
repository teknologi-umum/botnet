using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands.Soundtrack {
	public sealed record SoundtrackCommand : ICommand {
		public SlashCommand Command { get; }

		private SoundtrackCommand(SlashCommand command) {
			Command = command;
		}

		public static SoundtrackCommand FromSlashCommand(SlashCommand command) {
			return new(command);
		}
	}
}
