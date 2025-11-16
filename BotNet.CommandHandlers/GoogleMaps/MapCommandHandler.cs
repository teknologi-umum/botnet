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
		PlacesClient placesClient,
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

					// Try to get detailed place information from Places API
					PlaceDetails? placeDetails = null;
					if (!string.IsNullOrEmpty(primaryResult.Place_Id)) {
						try {
							placeDetails = await placesClient.GetPlaceDetailsAsync(
								primaryResult.Place_Id,
								CancellationToken.None
							);
						} catch (Exception exc) {
							logger.LogWarning(exc, "Could not get place details for {PlaceId}", primaryResult.Place_Id);
						}
					}

					// Calculate zoom level from viewport, default to 13
					int zoom = StaticMap.CalculateZoomLevel(primaryResult.Geometry.Viewport);

					// Generate static map URL
					string staticMapUrl = staticMap.GetMapUrl(lat, lng, zoom);

					// Build caption with place information
					StringBuilder caption = new();
					
					// Use place name from Places API if available, otherwise extract from formatted_address
					string placeName = placeDetails?.Name 
						?? primaryResult.Formatted_Address?.Split(',')[0].Trim() 
						?? command.PlaceName;
					caption.AppendLine($"📍 <b>{placeName}</b>");
					
					// Show address
					string address = primaryResult.Formatted_Address ?? "";
					caption.AppendLine($"{address}");
					caption.AppendLine();

					// Add rating if available
					if (placeDetails?.Rating != null && placeDetails.User_Ratings_Total != null) {
						string stars = new string('⭐', (int)Math.Round(placeDetails.Rating.Value));
						caption.AppendLine($"{stars} {placeDetails.Rating:F1} ({placeDetails.User_Ratings_Total:N0} reviews)");
					}

					// Add business status if available
					if (!string.IsNullOrEmpty(placeDetails?.Business_Status)) {
						string statusEmoji = placeDetails.Business_Status switch {
							"OPERATIONAL" => "✅",
							"CLOSED_TEMPORARILY" => "⏸️",
							"CLOSED_PERMANENTLY" => "❌",
							_ => "ℹ️"
						};
						caption.AppendLine($"{statusEmoji} {placeDetails.Business_Status.Replace("_", " ").ToLowerInvariant()}");
					}

					// Add opening hours if available
					if (placeDetails?.Opening_Hours?.Open_Now != null) {
						string openStatus = placeDetails.Opening_Hours.Open_Now.Value ? "🟢 Open now" : "🔴 Closed";
						caption.AppendLine(openStatus);
					}

					// Add price level if available
					if (placeDetails?.Price_Level != null) {
						string priceSymbol = string.Concat(Enumerable.Repeat("💰", placeDetails.Price_Level.Value));
						caption.AppendLine($"{priceSymbol}");
					}

					// Add editorial summary if available
					if (!string.IsNullOrEmpty(placeDetails?.Editorial_Summary?.Overview)) {
						caption.AppendLine();
						caption.AppendLine($"<i>{placeDetails.Editorial_Summary.Overview}</i>");
					}

					caption.AppendLine();
					
					// Add place types if available (and no detailed info from Places API)
					if (placeDetails == null && primaryResult.Types != null && primaryResult.Types.Length > 0) {
						string placeTypes = string.Join(", ", primaryResult.Types
							.Take(3)
							.Select(t => t.Replace("_", " "))
						);
						caption.AppendLine($"🏷️ <i>{placeTypes}</i>");
						caption.AppendLine();
					}

					// Add contact information if available
					if (!string.IsNullOrEmpty(placeDetails?.Formatted_Phone_Number)) {
						caption.AppendLine($"� {placeDetails.Formatted_Phone_Number}");
					}

					if (!string.IsNullOrEmpty(placeDetails?.Website)) {
						caption.AppendLine($"🌐 <a href=\"{placeDetails.Website}\">Website</a>");
					}

					if (!string.IsNullOrEmpty(placeDetails?.Formatted_Phone_Number) || !string.IsNullOrEmpty(placeDetails?.Website)) {
						caption.AppendLine();
					}

					caption.AppendLine($"�📐 Coordinates: <code>{lat:F6}, {lng:F6}</code>");
					caption.AppendLine($"🔍 Zoom Level: {zoom}");
					caption.AppendLine();
					
					// Link to Google Maps page if available from Places API
					if (!string.IsNullOrEmpty(placeDetails?.Url)) {
						caption.AppendLine($"<a href=\"{placeDetails.Url}\">🗺️ View in Google Maps</a>");
					} else {
						caption.AppendLine($"<a href=\"https://www.google.com/maps/search/{lat},{lng}\">🗺️ View in Google Maps</a>");
					}

					// Add other results if multiple places found
					if (results.Count > 1) {
						caption.AppendLine();
						caption.AppendLine("<b>Other matching places:</b>");
						
						int otherPlacesCount = Math.Min(3, results.Count - 1);
						for (int i = 1; i <= otherPlacesCount; i++) {
							Result otherResult = results[i];
							double otherLat = otherResult.Geometry!.Location!.Lat;
							double otherLng = otherResult.Geometry!.Location!.Lng;
							
							// Extract place name from formatted address
							string otherPlaceName = otherResult.Formatted_Address?.Split(',')[0].Trim() ?? command.PlaceName;
							
							caption.AppendLine($"{i}. <b>{otherPlaceName}</b>");
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
