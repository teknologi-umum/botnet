using BotNet.Commands.Weather;
using BotNet.Services.RateLimit;
using BotNet.Services.Weather;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Weather {
	public sealed class WeatherCommandHandler(
		ITelegramBotClient telegramBotClient,
		CurrentWeather currentWeather,
		ILogger<WeatherCommandHandler> logger
	) : ICommandHandler<WeatherCommand> {
		private static readonly RateLimiter GetWeatherRateLimiter = RateLimiter.PerUserPerChat(3, TimeSpan.FromMinutes(2));

		public Task Handle(WeatherCommand command, CancellationToken cancellationToken) {
			try {
				GetWeatherRateLimiter.ValidateActionRate(command.Chat.Id, command.Sender.Id);
			} catch (RateLimitExceededException exc) {
				return telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"Anda belum mendapat giliran. Coba lagi {exc.Cooldown}.",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
					cancellationToken: cancellationToken
				);
			}

			// Fire and forget
			Task.Run(async () => {
				try {
					(string title, string icon) = await currentWeather.GetCurrentWeatherAsync(
						place: command.CityName,
						cancellationToken: cancellationToken
					);

					await telegramBotClient.SendPhoto(
						chatId: command.Chat.Id,
						photo: new InputFileUrl(icon),
						caption: title,
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
						cancellationToken: cancellationToken
					);
				} catch (OperationCanceledException) {
					// Terminate gracefully
				} catch (Exception exc) {
					logger.LogError(exc, "Could not get weather");
					await telegramBotClient.SendMessage(
						chatId: command.Chat.Id,
						text: "<code>Lokasi tidak dapat ditemukan</code>",
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
						cancellationToken: CancellationToken.None
					);
				}
			});

			return Task.CompletedTask;
		}
	}
}
