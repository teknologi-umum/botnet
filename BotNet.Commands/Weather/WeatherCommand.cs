using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Common;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Weather {
	public sealed record WeatherCommand : ICommand {
		public string CityName { get; }
		public MessageId CommandMessageId { get; }
		public ChatBase Chat { get; }
		public HumanSender Sender { get; }

		public WeatherCommand(
			string cityName,
			MessageId commandMessageId,
			ChatBase chat,
			HumanSender sender
		) {
			CityName = cityName;
			CommandMessageId = commandMessageId;
			Chat = chat;
			Sender = sender;
		}

		public static WeatherCommand FromSlashCommand(SlashCommand slashCommand) {
			// Must be /weather
			if (slashCommand.Command != "/weather") {
				throw new ArgumentException("Command must be /weather.", nameof(slashCommand));
			}

			// Default to Jakarta if no city name provided
			string cityName = string.IsNullOrWhiteSpace(slashCommand.Text) ? "Jakarta" : slashCommand.Text;

			return new(
				cityName: cityName,
				commandMessageId: slashCommand.MessageId,
				chat: slashCommand.Chat,
				sender: slashCommand.Sender
			);
		}
	}
}
