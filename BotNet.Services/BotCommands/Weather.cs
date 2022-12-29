using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.RateLimit;
using BotNet.Services.Weather;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Weather {
		private static readonly RateLimiter GET_WEATHER_LIMITER = RateLimiter.PerUserPerChat(3, TimeSpan.FromMinutes(2));

		public static async Task GetWeatherAsync(ITelegramBotClient telegramBotClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {

				if (commandArgument.Length > 0) {
					try {
						GET_WEATHER_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);

						try {
							(string title, string icon) = await serviceProvider.GetRequiredService<CurrentWeather>().GetCurrentWeatherAsync(commandArgument);

							await telegramBotClient.SendPhotoAsync(
								chatId: message.Chat.Id,
								photo: icon,
								caption: title,
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} catch {
							await telegramBotClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Lokasi tidak dapat ditemukan</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId);
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
						text: "<code>Silakan masukkan lokasi di depan perintah /weather</code>",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			} else {
				await telegramBotClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "<code>Silakan masukkan lokasi di depan perintah /weather</code>",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			}
		}
	}
}
