using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.RateLimit;
using BotNet.Services.Stability.Models;
using BotNet.Services.ThisXDoesNotExist;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Art {
		private static readonly RateLimiter RANDOM_ART_RATE_LIMITER = RateLimiter.PerChat(2, TimeSpan.FromMinutes(2));
		private static readonly RateLimiter GENERATED_ART_RATE_LIMITER = RateLimiter.PerUserPerChat(1, TimeSpan.FromMinutes(5));
		private static readonly RateLimiter MODIFY_ART_RATE_LIMITER = RateLimiter.PerUserPerChat(1, TimeSpan.FromMinutes(5));

		public static async Task GetRandomArtAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					try {
						GENERATED_ART_RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);

						Message busyMessage = await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "Generating image… ⏳",
							parseMode: ParseMode.Markdown,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken
						);

						try {
							byte[] image = await serviceProvider.GetRequiredService<Stability.Skills.ImageGenerationBot>().GenerateImageAsync(commandArgument, CancellationToken.None);
							using MemoryStream imageStream = new(image);

							try {
								await botClient.DeleteMessageAsync(
									chatId: busyMessage.Chat.Id,
									messageId: busyMessage.MessageId,
									cancellationToken: cancellationToken
								);
							} catch (OperationCanceledException) {
								throw;
							}

							await botClient.SendPhotoAsync(
								chatId: message.Chat.Id,
								photo: new InputFileStream(imageStream, "art.png"),
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} catch (ContentFilteredException) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Content filtered</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} catch {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Could not generate art</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
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
						photo: new InputFileStream(imageStream, "art.jpg"),
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

		//public static async Task ModifyArtAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, string textPrompt, CancellationToken cancellationToken) {
		//	if (message.ReplyToMessage is { } replyToMessage) {
		//		using MemoryStream originalImageStream = new();
		//		Telegram.Bot.Types.File fileInfo = message.ReplyToMessage.Photo?.Length > 0
		//			? await botClient.GetInfoAndDownloadFileAsync(
		//				fileId: message.ReplyToMessage.Photo.OrderByDescending(photoSize => photoSize.Width).First().FileId,
		//				destination: originalImageStream,
		//				cancellationToken: cancellationToken)
		//			: await botClient.GetInfoAndDownloadFileAsync(
		//				fileId: message.ReplyToMessage.Sticker!.FileId,
		//				destination: originalImageStream,
		//				cancellationToken: cancellationToken);

		//		try {
		//			MODIFY_ART_RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);

		//			try {
		//				byte[] image = await serviceProvider.GetRequiredService<StabilityClient>().ModifyImageAsync(originalImageStream.ToArray(), textPrompt, CancellationToken.None);
		//				using MemoryStream imageStream = new(image);

		//				await botClient.SendPhotoAsync(
		//					chatId: message.Chat.Id,
		//					photo: new InputFileStream(imageStream, "art.jpg"),
		//					replyToMessageId: message.MessageId,
		//					cancellationToken: cancellationToken);
		//			} catch {
		//				await botClient.SendTextMessageAsync(
		//					chatId: message.Chat.Id,
		//					text: "<code>Could not generate art</code>",
		//					parseMode: ParseMode.Html,
		//					replyToMessageId: message.MessageId,
		//					cancellationToken: cancellationToken);
		//			}
		//		} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
		//			await botClient.SendTextMessageAsync(
		//				chatId: message.Chat.Id,
		//				text: $"Anda belum mendapat giliran. Coba lagi {cooldown}.",
		//				parseMode: ParseMode.Html,
		//				replyToMessageId: message.MessageId,
		//				cancellationToken: cancellationToken);
		//		}
		//	}
		//}
	}
}
