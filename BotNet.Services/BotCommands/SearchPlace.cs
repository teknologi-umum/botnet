using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.GoogleMap;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class SearchPlace {
		private static readonly RateLimiter SEARCH_PLACE_LIMITER = RateLimiter.PerUserPerChat(1, TimeSpan.FromMinutes(2));

		public static async Task SearchPlaceAsync(ITelegramBotClient telegramBotClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {

				if (commandArgument.Length > 0) {
					try {
						SEARCH_PLACE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);

						try {
							string coords = await serviceProvider.GetRequiredService<GeoCode>().SearchPlaceAsync(commandArgument);
							string staticMapUrl = serviceProvider.GetRequiredService<StaticMap>().SearchPlace(commandArgument);

							await telegramBotClient.SendPhotoAsync(
								chatId: message.Chat.Id,
								photo: new InputFileUrl(staticMapUrl),
								caption: coords,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} catch {
							await telegramBotClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Lokasi tidak dapat ditemukan</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						}
					} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
						await telegramBotClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"Anda belum mendapat giliran. Coba lagi {cooldown}.",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else {
					await telegramBotClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: "<code>Silakan masukkan lokasi di depan perintah /map</code>",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			} else {
				await telegramBotClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "<code>Silakan masukkan lokasi di depan perintah /map</code>",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			}
		}
	}
}
