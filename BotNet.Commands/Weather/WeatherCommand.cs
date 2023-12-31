using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.Common;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Weather {
	public sealed record WeatherCommand : ICommand {
		public string CityName { get; }
		public int CommandMessageId { get; }
		public long ChatId { get; }
		public long SenderId { get; }

		public WeatherCommand(
			string cityName,
			int commandMessageId,
			long chatId,
			long senderId
		) {
			CityName = cityName;
			CommandMessageId = commandMessageId;
			ChatId = chatId;
			SenderId = senderId;
		}

		public static WeatherCommand FromSlashCommand(SlashCommand slashCommand) {
			// Must be /weather
			if (slashCommand.Command != "/weather") {
				throw new ArgumentException("Command must be /weather.", nameof(slashCommand));
			}

			// City name must be non-empty
			if (string.IsNullOrWhiteSpace(slashCommand.Text)) {
				throw new UsageException(
					message: "Silakan masukkan nama kota setelah perintah `/weather`\\.",
					parseMode: ParseMode.MarkdownV2,
					commandMessageId: slashCommand.MessageId
				);
			}

			return new(
				cityName: slashCommand.Text,
				commandMessageId: slashCommand.MessageId,
				chatId: slashCommand.ChatId,
				senderId: slashCommand.SenderId
			);
		}
	}
}
