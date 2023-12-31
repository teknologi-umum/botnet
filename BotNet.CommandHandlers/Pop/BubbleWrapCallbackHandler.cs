using BotNet.Commands.Pop;
using BotNet.Services.BubbleWrap;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotNet.CommandHandlers.Pop {
	public sealed class BubbleWrapCallbackHandler(
		ITelegramBotClient telegramBotClient,
		BubbleWrapKeyboardGenerator bubbleWrapKeyboardGenerator
	) : ICommandHandler<BubbleWrapCallback> {
		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly BubbleWrapKeyboardGenerator _bubbleWrapKeyboardGenerator = bubbleWrapKeyboardGenerator;

		public Task Handle(BubbleWrapCallback command, CancellationToken cancellationToken) {
			InlineKeyboardMarkup poppedKeyboardMarkup = _bubbleWrapKeyboardGenerator.HandleCallback(
				chatId: command.ChatId,
				messageId: command.MessageId,
				sheetData: command.SheetData
			);

			// Fire and forget
			Task.Run(async () => {
				await _telegramBotClient.EditMessageReplyMarkupAsync(
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
