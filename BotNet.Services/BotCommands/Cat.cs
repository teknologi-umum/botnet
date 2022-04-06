using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.RateLimit;
using BotNet.Services.ThisXDoesNotExist;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace BotNet.Services.BotCommands {
	public static class Cat {
		private static readonly RateLimiter RATE_LIMITER = RateLimiter.PerChat(2, TimeSpan.FromMinutes(2));
		public static async Task GetRandomCatAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			try {
				RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);

				byte[] image = await serviceProvider.GetRequiredService<ThisCatDoesNotExist>().GetRandomCatImageAsync(cancellationToken);
				using MemoryStream imageStream = new(image);

				await botClient.SendPhotoAsync(
					chatId: message.Chat.Id,
					photo: new InputOnlineFile(imageStream, "cat.jpg"),
					cancellationToken: cancellationToken);
			} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: $"Persediaan kucing habis. Coba lagi {cooldown}.",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			}
		}
	}
}
