using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Meme;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Meme {
		public static async Task HandleRamadAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities is { Length: 1 } entities
				&& entities[0] is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				byte[] generatedImage = serviceProvider.GetRequiredService<MemeGenerator>().CaptionRamad(commandArgument);
				using MemoryStream floppedImageStream = new(generatedImage);

				await botClient.SendPhotoAsync(
				chatId: message.Chat.Id,
					photo: new InputFileStream(floppedImageStream, "ramad.png"),
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			} else {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "Untuk menyuruh Riza presentasi, silakan ketik /ramad diikuti judul presentasi.",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			}
		}
	}
}
