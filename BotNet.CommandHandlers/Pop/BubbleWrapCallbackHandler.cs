using BotNet.Commands.Pop;
using Mediator;
using BotNet.Services.BubbleWrap;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotNet.CommandHandlers.Pop {
	public sealed class BubbleWrapCallbackHandler(
		ITelegramBotClient telegramBotClient,
		BubbleWrapKeyboardGenerator bubbleWrapKeyboardGenerator,
		ILogger<BubbleWrapCallbackHandler> logger
	) : ICommandHandler<BubbleWrapCallback> {
		public ValueTask<Unit> Handle(BubbleWrapCallback command, CancellationToken cancellationToken) {
			InlineKeyboardMarkup poppedKeyboardMarkup = bubbleWrapKeyboardGenerator.HandleCallback(
				chatId: command.ChatId,
				messageId: command.MessageId,
				sheetData: command.SheetData
			);

			// Fire and forget
			BackgroundTask.Run(async () => {
				await telegramBotClient.EditMessageReplyMarkup(
					chatId: command.ChatId,
					messageId: command.MessageId,
					replyMarkup: poppedKeyboardMarkup,
					cancellationToken: cancellationToken
				);
			}, logger);

			return default;
		}
	}
}
