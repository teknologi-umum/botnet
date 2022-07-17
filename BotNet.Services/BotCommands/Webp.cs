using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types;
using Telegram.Bot;
using BotNet.Services.Webp;

namespace BotNet.Services.BotCommands {
	public static class Webp {
		public static async Task ConvertToImageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
			if (message.ReplyToMessage is null) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "Apa yang mau diconvert? Untuk convert gambar, reply `/webp` ke pesan yang ada stickernya\\.",
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			} else if (message.ReplyToMessage.Sticker is null) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "Pesan ini tidak ada stickernya\\. Untuk convert gambar, reply `/webp` ke pesan yang ada stickernya\\.",
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			} else {
				using MemoryStream originalImageStream = new();
				Telegram.Bot.Types.File fileInfo = await botClient.GetInfoAndDownloadFileAsync(
					fileId: message.ReplyToMessage.Sticker!.FileId,
					destination: originalImageStream,
					cancellationToken: cancellationToken);

				byte[] image = WebpToImageConverter.Convert(originalImageStream.ToArray());
				using MemoryStream imageStream = new(image);

				await botClient.SendPhotoAsync(
					chatId: message.Chat.Id,
					photo: new InputOnlineFile(imageStream, new string(fileInfo.FileId.Reverse().ToArray()) + ".png"),
					replyToMessageId: message.ReplyToMessage.MessageId,
					cancellationToken: cancellationToken);
			}
		}
	}
}
