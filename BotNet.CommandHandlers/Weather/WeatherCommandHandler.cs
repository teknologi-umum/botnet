using BotNet.Commands.Weather;
using BotNet.Services.RateLimit;
using BotNet.Services.Weather;
using BotNet.Services.Weather.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Weather {
	public sealed class WeatherCommandHandler(
		ITelegramBotClient telegramBotClient,
		WttrInWeather wttrInWeather,
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
			BackgroundTask.Run(async () => {
				try {
					WttrInResponse? weatherResponse = await wttrInWeather.GetWeatherAsync(
						location: command.CityName,
						cancellationToken: cancellationToken
					);

					if (weatherResponse == null) {
						await telegramBotClient.SendMessage(
							chatId: command.Chat.Id,
							text: "<code>Tidak dapat mengambil data cuaca</code>",
							parseMode: ParseMode.Html,
							replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
							cancellationToken: CancellationToken.None
						);
						return;
					}

					string weatherReport = WttrInWeather.FormatWeatherReport(weatherResponse, command.CityName);

					await telegramBotClient.SendMessage(
						chatId: command.Chat.Id,
						text: weatherReport,
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
						cancellationToken: cancellationToken
					);
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
			}, logger);

			return Task.CompletedTask;
		}
	}
}
