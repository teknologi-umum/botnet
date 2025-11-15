using BotNet.Commands.GoogleMaps;
using BotNet.Services.GoogleMap;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.GoogleMaps {
	public sealed class MapCommandHandler(
		ITelegramBotClient telegramBotClient,
		GeoCode geoCode,
		StaticMap staticMap,
		ILogger<MapCommandHandler> logger
	) : ICommandHandler<MapCommand> {
		private static readonly RateLimiter SearchPlaceRateLimiter = RateLimiter.PerUserPerChat(1, TimeSpan.FromMinutes(2));

		public Task Handle(MapCommand command, CancellationToken cancellationToken) {
			try {
				SearchPlaceRateLimiter.ValidateActionRate(command.Chat.Id, command.Sender.Id);
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
					(double lat, double lng) = await geoCode.SearchPlaceAsync(command.PlaceName);
					string staticMapUrl = staticMap.SearchPlace(command.PlaceName);

					await telegramBotClient.SendPhoto(
						chatId: command.Chat.Id,
						photo: new InputFileUrl(staticMapUrl),
						caption: $"<a href=\"https://www.google.com/maps/search/{lat},{lng}\">View in 🗺️ Google Maps</a>",
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
						cancellationToken: cancellationToken
					);
				} catch (Exception exc) {
					logger.LogError(exc, "Could not find place");
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
