using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Hosting;
using BotNet.Services.OCR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Read {
		public static async Task HandleReadAsync(IServiceProvider serviceProvider, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
			long memory = serviceProvider.GetRequiredService<IOptions<HostingOptions>>().Value.Memory;

			// Limit image quality based on VM memory size
			(int maxFileSize, int maxImageArea) = memory switch {
				< 500_000_000L => (100_000, 10_000),
				< 1_500_000_000L => (1_000_000, 800_000),
				_ => (2_000_000, 2_000_000)
			};

			if (message.ReplyToMessage is null) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "Apa yang mau diread? Untuk ngeread gambar, reply `/read` ke pesan yang ada gambarnya\\.",
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			} else if ((message.ReplyToMessage.Photo is null || message.ReplyToMessage.Photo.Length == 0)
				&& message.ReplyToMessage.Sticker is null) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "Pesan ini tidak ada gambarnya\\. Untuk ngeread gambar, reply `/read` ke pesan yang ada gambarnya\\.",
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			} else {
				string? fileId = message.ReplyToMessage.Photo?
					.Where(photoSize => photoSize.FileSize < maxFileSize && photoSize.Width * photoSize.Height < maxImageArea)
					.OrderByDescending(photoSize => photoSize.FileSize)
					.FirstOrDefault()?.FileId
					?? (message.ReplyToMessage.Sticker is {
						IsAnimated: false,
						FileSize: int stickerFileSize,
						Width: int stickerWidth,
						Height: int stickerHeight,
						FileId: string stickerFileId
					}
					&& stickerFileSize < maxFileSize
					&& stickerWidth * stickerHeight < maxImageArea
					? stickerFileId
					: null);

				if (fileId is null) {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: "Gambarnya terlalu besar\\.",
						parseMode: ParseMode.MarkdownV2,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
					return;
				}

				using MemoryStream originalImageStream = new();
				Telegram.Bot.Types.File fileInfo = await botClient.GetInfoAndDownloadFileAsync(
					fileId: fileId,
					destination: originalImageStream,
					cancellationToken: cancellationToken);

				GC.Collect();
				string textResult = await serviceProvider
					.GetRequiredService<Reader>()
					.ReadImageAsync(originalImageStream.ToArray(), cancellationToken);
				GC.Collect();

				if (textResult.Length == 0) {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: "Gambarnya sulit dibaca 🙁",
						parseMode: ParseMode.MarkdownV2,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
					return;
				}

				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: textResult
						.Replace("\\", "\\\\", StringComparison.InvariantCultureIgnoreCase)
						.Replace(".", "\\.", StringComparison.InvariantCultureIgnoreCase)
						.Replace("|", "\\|", StringComparison.InvariantCultureIgnoreCase)
						.Replace("!", "\\!", StringComparison.InvariantCultureIgnoreCase)
						.Replace("[", "\\[", StringComparison.InvariantCultureIgnoreCase)
						.Replace("]", "\\]", StringComparison.InvariantCultureIgnoreCase)
						.Replace("(", "\\(", StringComparison.InvariantCultureIgnoreCase)
						.Replace(")", "\\)", StringComparison.InvariantCultureIgnoreCase)
						.Replace("{", "\\{", StringComparison.InvariantCultureIgnoreCase)
						.Replace("}", "\\}", StringComparison.InvariantCultureIgnoreCase)
						.Replace("_", "\\_", StringComparison.InvariantCultureIgnoreCase)
						.Replace("-", "\\-", StringComparison.InvariantCultureIgnoreCase)
						.Replace("~", "\\~", StringComparison.InvariantCultureIgnoreCase)
						.Replace("=", "\\=", StringComparison.InvariantCultureIgnoreCase)
						.Replace("*", "\\*", StringComparison.InvariantCultureIgnoreCase)
						.Replace("#", "\\#", StringComparison.InvariantCultureIgnoreCase)
						.Replace("/", "\\/", StringComparison.InvariantCultureIgnoreCase)
						.Replace("`", "\\`", StringComparison.InvariantCultureIgnoreCase)
						.Replace("&", "\\&", StringComparison.InvariantCultureIgnoreCase)
						.Replace("<", "\\<", StringComparison.InvariantCultureIgnoreCase)
						.Replace(">", "\\>", StringComparison.InvariantCultureIgnoreCase),
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: message.ReplyToMessage.From!.IsBot
						? message.MessageId
						: message.ReplyToMessage.MessageId,
					cancellationToken: cancellationToken);
			}
		}
	}
}
