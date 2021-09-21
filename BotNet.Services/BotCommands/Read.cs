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
				using MemoryStream originalImageStream = new();
				Telegram.Bot.Types.File fileInfo = await botClient.GetInfoAndDownloadFileAsync(
					fileId: message.ReplyToMessage.Photo.OrderBy(photoSize => photoSize.Width).First().FileId,
					destination: originalImageStream,
					cancellationToken: cancellationToken);

				GC.Collect();
				string textResult = await serviceProvider
					.GetRequiredService<Reader>()
					.ReadImageAsync(originalImageStream.ToArray(), cancellationToken);
				GC.Collect();

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
					replyToMessageId: message.ReplyToMessage.MessageId,
					cancellationToken: cancellationToken);
			}
		}
	}
}
