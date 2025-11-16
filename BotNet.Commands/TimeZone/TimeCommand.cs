using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.Common;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.TimeZone {
	public sealed record TimeCommand : ICommand {
		public string CityOrTimeZone { get; }
		public SlashCommand Command { get; }

		private TimeCommand(string cityOrTimeZone, SlashCommand command) {
			CityOrTimeZone = cityOrTimeZone;
			Command = command;
		}

		public static TimeCommand FromSlashCommand(SlashCommand command) {
			string cityOrTimeZone = command.Text.Trim();
			if (string.IsNullOrWhiteSpace(cityOrTimeZone)) {
				cityOrTimeZone = "Jakarta";
			}

			return new TimeCommand(cityOrTimeZone, command);
		}
	}
}
