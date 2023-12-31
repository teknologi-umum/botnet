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
		private static readonly RateLimiter GET_WEATHER_RATE_LIMITER = RateLimiter.PerUserPerChat(3, TimeSpan.FromMinutes(2));

		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly CurrentWeather _currentWeather = currentWeather;
		private readonly ILogger<WeatherCommandHandler> _logger = logger;

		public Task Handle(WeatherCommand command, CancellationToken cancellationToken) {
			try {
				GET_WEATHER_RATE_LIMITER.ValidateActionRate(command.ChatId, command.SenderId);
			} catch (RateLimitExceededException exc) {
				return _telegramBotClient.SendTextMessageAsync(
					chatId: command.ChatId,
					text: $"Anda belum mendapat giliran. Coba lagi {exc.Cooldown}.",
					parseMode: ParseMode.Html,
					replyToMessageId: command.CommandMessageId,
					cancellationToken: cancellationToken
				);
			}

			// Fire and forget
			Task.Run(async () => {
				try {
					(string title, string icon) = await _currentWeather.GetCurrentWeatherAsync(
						place: command.CityName,
						cancellationToken: cancellationToken
					);

					await _telegramBotClient.SendPhotoAsync(
						chatId: command.ChatId,
						photo: new InputFileUrl(icon),
						caption: title,
						parseMode: ParseMode.Html,
						replyToMessageId: command.CommandMessageId,
						cancellationToken: cancellationToken
					);
				} catch (OperationCanceledException) {
					// Terminate gracefully
				} catch (Exception exc) {
					_logger.LogError(exc, "Could not get weather");
					await _telegramBotClient.SendTextMessageAsync(
						chatId: command.ChatId,
						text: "<code>Lokasi tidak dapat ditemukan</code>",
						parseMode: ParseMode.Html,
						replyToMessageId: command.CommandMessageId,
						cancellationToken: CancellationToken.None
					);
				}
			});

			return Task.CompletedTask;
		}
	}
}
