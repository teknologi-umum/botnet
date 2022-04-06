using System;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.RateLimit;
using BotNet.Services.ThisXDoesNotExist;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace BotNet.Services.BotCommands {
	public static class Waifu {
		private static readonly RateLimiter RATE_LIMITER = RateLimiter.PerChat(2, TimeSpan.FromMinutes(2));
		public static async Task GetRandomWaifuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
			try {
				RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);

				Uri randomUrl = ThisWaifuDoesNotExist.GetRandomUrl();

				await botClient.SendPhotoAsync(
					chatId: message.Chat.Id,
					photo: new InputOnlineFile(randomUrl),
					cancellationToken: cancellationToken);
			} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: $"Persediaan waifu habis. Coba lagi {cooldown}.",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			}
		}
	}
}
