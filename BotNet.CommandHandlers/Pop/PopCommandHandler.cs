using BotNet.Commands.Pop;
using BotNet.Services.BubbleWrap;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Pop {
	public sealed class PopCommandHandler(
		ITelegramBotClient telegramBotClient
	) : ICommandHandler<PopCommand> {
		public async Task Handle(PopCommand command, CancellationToken cancellationToken) {
			await telegramBotClient.SendMessage(
				chatId: command.Chat.Id,
				text: "Here's a bubble wrap. Enjoy!",
				parseMode: ParseMode.Html,
				replyMarkup: BubbleWrapKeyboardGenerator.EmptyKeyboard,
				cancellationToken: cancellationToken
			);
		}
	}
}
