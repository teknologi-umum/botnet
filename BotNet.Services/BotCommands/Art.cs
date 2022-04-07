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
	public static class Art {
		private static readonly RateLimiter RATE_LIMITER = RateLimiter.PerChat(2, TimeSpan.FromMinutes(2));
		public static async Task GetRandomArtAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			try {
				RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);

				byte[] image = await serviceProvider.GetRequiredService<ThisArtworkDoesNotExist>().GetRandomArtworkAsync(cancellationToken);
				using MemoryStream imageStream = new(image);

				await botClient.SendPhotoAsync(
					chatId: message.Chat.Id,
					photo: new InputOnlineFile(imageStream, "art.jpg"),
					cancellationToken: cancellationToken);
			} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: $"Saya belum selesai melukis. Coba lagi {cooldown}.",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			}
		}
	}
}
