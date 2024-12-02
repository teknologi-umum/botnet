using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Preview;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	[Obsolete("Should be refactored to PreviewCommandHandler later")]
	public static class Preview {
		public static async Task GetPreviewAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {

				if (commandArgument.Length <= 0 && message.ReplyToMessage?.Text is null) {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: "Gunakan <code>/preview youtoube link </code> atau reply message dengan command <code>/preview</code>",
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters {
							MessageId = message.MessageId
						},
						cancellationToken: cancellationToken);

					return;
				}

				Uri? youtubeLink;
				Uri? previewYoutubeStoryboard;

				if (message.ReplyToMessage?.Text is string repliedToMessage) {
					youtubeLink = YoutubePreview.ValidateYoutubeLink(repliedToMessage);
					if (youtubeLink is null) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "Youtube link tidak valid",
							parseMode: ParseMode.Html,
							replyParameters: new ReplyParameters {
								MessageId = message.MessageId
							},
							cancellationToken: cancellationToken);

						return;
					}

					previewYoutubeStoryboard = await serviceProvider.GetRequiredService<YoutubePreview>().YoutubeStoryBoardAsync(youtubeLink, cancellationToken);

					await botClient.SendPhotoAsync(
						chatId: message.Chat.Id,
						photo: new InputFileUrl(previewYoutubeStoryboard),
						replyParameters: new ReplyParameters {
							MessageId = message.MessageId
						},
						parseMode: ParseMode.Html,
						cancellationToken: cancellationToken);
				} else if (commandArgument.Length >= 0) {
					youtubeLink = YoutubePreview.ValidateYoutubeLink(commandArgument);
					if (youtubeLink is null) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "Youtube link tidak valid",
							parseMode: ParseMode.Html,
							replyParameters: new ReplyParameters {
								MessageId = message.MessageId
							},
							cancellationToken: cancellationToken);

						return;
					}

					previewYoutubeStoryboard = await serviceProvider.GetRequiredService<YoutubePreview>().YoutubeStoryBoardAsync(youtubeLink, cancellationToken);

					await botClient.SendPhotoAsync(
						chatId: message.Chat.Id,
						photo: new InputFileUrl(previewYoutubeStoryboard),
						replyParameters: new ReplyParameters {
							MessageId = message.MessageId
						},
						parseMode: ParseMode.Html,
						cancellationToken: cancellationToken);
				}
			}
		}
	}
}
