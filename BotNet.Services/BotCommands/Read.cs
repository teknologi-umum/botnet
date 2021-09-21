using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.OCR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Read {
		private const int MAX_IMAGE_AREA = 100_000;
		private const int MAX_FILE_SIZE = 10_000;

		public static async Task HandleReadAsync(IServiceProvider serviceProvider, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
			if (message.ReplyToMessage is null) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "Apa yang mau diread? Untuk ngeread gambar, reply `/read` ke pesan yang ada gambarnya\\.",
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			} else if (message.ReplyToMessage.Photo is null || message.ReplyToMessage.Photo.Length == 0) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "Pesan ini tidak ada gambarnya\\. Untuk ngeread gambar, reply `/read` ke pesan yang ada gambarnya\\.",
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			} else {
				string? fileId = message.ReplyToMessage.Photo
					.Where(photoSize => photoSize.FileSize < MAX_FILE_SIZE && photoSize.Width * photoSize.Height < MAX_IMAGE_AREA)
					.OrderByDescending(photoSize => photoSize.FileSize)
					.FirstOrDefault()?.FileId;

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
