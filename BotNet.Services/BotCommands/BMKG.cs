using System;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.BMKG;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class BMKG {
		private static readonly RateLimiter RATE_LIMITER = RateLimiter.PerChat(3, TimeSpan.FromMinutes(2));
		public static async Task GetLatestEarthQuakeAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			try {
				RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);

				(string text, string shakemapUrl) = await serviceProvider.GetRequiredService<LatestEarthQuake>().GetLatestAsync();

				await botClient.SendPhotoAsync(
					chatId: message.Chat.Id,
					photo: new InputFileUrl(shakemapUrl),
					caption: text,
					replyToMessageId: message.MessageId,
					parseMode: ParseMode.Html,
					cancellationToken: cancellationToken);
			} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: $"Sabar dulu ya, tunggu giliran yang lain. Coba lagi {cooldown}.",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			}
		}
	}
}
