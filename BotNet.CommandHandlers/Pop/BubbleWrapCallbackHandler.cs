using BotNet.Commands.Pop;
using BotNet.Services.BubbleWrap;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotNet.CommandHandlers.Pop {
	public sealed class BubbleWrapCallbackHandler(
		ITelegramBotClient telegramBotClient,
		BubbleWrapKeyboardGenerator bubbleWrapKeyboardGenerator
	) : ICommandHandler<BubbleWrapCallback> {
		public Task Handle(BubbleWrapCallback command, CancellationToken cancellationToken) {
			InlineKeyboardMarkup poppedKeyboardMarkup = bubbleWrapKeyboardGenerator.HandleCallback(
				chatId: command.ChatId,
				messageId: command.MessageId,
				sheetData: command.SheetData
			);

			// Fire and forget
			Task.Run(async () => {
				await telegramBotClient.EditMessageReplyMarkup(
					chatId: command.ChatId,
					messageId: command.MessageId,
					replyMarkup: poppedKeyboardMarkup,
					cancellationToken: cancellationToken
				);
			});

			return Task.CompletedTask;
		}
	}
}
