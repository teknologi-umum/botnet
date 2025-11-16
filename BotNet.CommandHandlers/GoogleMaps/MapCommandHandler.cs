using BotNet.Commands.GoogleMaps;
using BotNet.Services.GoogleMap;
using BotNet.Services.GoogleMap.Models;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
					List<Result> results = await geoCode.SearchPlacesAsync(command.PlaceName);
					
					if (results.Count == 0) {
						await telegramBotClient.SendMessage(
							chatId: command.Chat.Id,
							text: "<code>Lokasi tidak dapat ditemukan</code>",
							parseMode: ParseMode.Html,
							replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
							cancellationToken: CancellationToken.None
						);
						return;
					}

					// Primary result (closest to Jakarta)
					Result primaryResult = results[0];
					double lat = primaryResult.Geometry!.Location!.Lat;
					double lng = primaryResult.Geometry!.Location!.Lng;

					// Calculate zoom level from viewport, default to 13
					int zoom = StaticMap.CalculateZoomLevel(primaryResult.Geometry.Viewport);

					// Generate static map URL
					string staticMapUrl = staticMap.GetMapUrl(lat, lng, zoom);

					// Build caption with place information
					StringBuilder caption = new();
					caption.AppendLine($"📍 <b>{command.PlaceName}</b>");
					caption.AppendLine($"{primaryResult.Formatted_Address}");
					caption.AppendLine();
					
					// Add place types if available
					if (primaryResult.Types != null && primaryResult.Types.Length > 0) {
						string placeTypes = string.Join(", ", primaryResult.Types
							.Take(3)
							.Select(t => t.Replace("_", " "))
						);
						caption.AppendLine($"🏷️ <i>{placeTypes}</i>");
						caption.AppendLine();
					}

					caption.AppendLine($"📐 Coordinates: <code>{lat:F6}, {lng:F6}</code>");
					caption.AppendLine($"🔍 Zoom Level: {zoom}");
					caption.AppendLine();
					caption.AppendLine($"<a href=\"https://www.google.com/maps/search/{lat},{lng}\">🗺️ View in Google Maps</a>");

					// Add other results if multiple places found
					if (results.Count > 1) {
						caption.AppendLine();
						caption.AppendLine("<b>Other matching places:</b>");
						
						int otherPlacesCount = Math.Min(3, results.Count - 1);
						for (int i = 1; i <= otherPlacesCount; i++) {
							Result otherResult = results[i];
							double otherLat = otherResult.Geometry!.Location!.Lat;
							double otherLng = otherResult.Geometry!.Location!.Lng;
							
							caption.AppendLine($"{i}. <b>{command.PlaceName}</b>");
							caption.AppendLine($"   {otherResult.Formatted_Address}");
							caption.AppendLine($"   <a href=\"https://www.google.com/maps/search/{otherLat},{otherLng}\">View on map</a>");
						}
						
						if (results.Count > 4) {
							caption.AppendLine($"   <i>...and {results.Count - 4} more</i>");
						}
					}

					await telegramBotClient.SendPhoto(
						chatId: command.Chat.Id,
						photo: new InputFileUrl(staticMapUrl),
						caption: caption.ToString(),
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
