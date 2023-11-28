using System;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.RateLimit;
using BotNet.Services.ThisXDoesNotExist;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Idea {
		private static readonly RateLimiter RATE_LIMITER = RateLimiter.PerChat(3, TimeSpan.FromMinutes(5));
		public static async Task GetRandomIdeaAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			try {
				RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);

				string? idea = await serviceProvider.GetRequiredService<ThisIdeaDoesNotExist>().GetRandomIdeaAsync(cancellationToken);

				if (idea is null) {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: $"Bentar ya saya mikir dulu idenya. Coba lagi nanti.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
					return;
				}

				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: $"Idea: {idea}",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: $"Bentar ya saya mikir dulu idenya. Coba lagi {cooldown}.",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			}
		}
	}
}
