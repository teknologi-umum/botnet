using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.RateLimit;
using BotNet.Services.Stability;
using BotNet.Services.ThisXDoesNotExist;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace BotNet.Services.BotCommands {
	public static class Art {
		private static readonly RateLimiter RANDOM_ART_RATE_LIMITER = RateLimiter.PerChat(2, TimeSpan.FromMinutes(2));
		private static readonly RateLimiter GENERATED_ART_RATE_LIMITER = RateLimiter.PerUserPerChat(1, TimeSpan.FromMinutes(5));
		private static readonly HashSet<int> HANDLED_MESSAGE_IDS = new();
		public static async Task GetRandomArtAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					try {
						GENERATED_ART_RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);

						DateTime started = DateTime.Now;
						Message responseMessage = await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "Generating art... Please wait a few seconds",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId);

						try {
							byte[] image = await serviceProvider.GetRequiredService<StabilityClient>().GenerateImageAsync(commandArgument, CancellationToken.None);
							using MemoryStream imageStream = new(image);

							await botClient.SendPhotoAsync(
								chatId: message.Chat.Id,
								photo: new InputOnlineFile(imageStream, "art.jpg"),
								cancellationToken: cancellationToken);
						} catch {
							await botClient.EditMessageTextAsync(
								chatId: message.Chat.Id,
								messageId: responseMessage.MessageId,
								text: "<code>Could not generate art</code>",
								parseMode: ParseMode.Html
							);
						}
					} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"Anda belum mendapat giliran. Coba lagi {cooldown}.",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				}
			} else {
				try {
					RANDOM_ART_RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);

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
}
