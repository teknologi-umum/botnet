using System.Net;
using BotNet.Commands.Fuck;
using BotNet.Services.Brainfuck;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Fuck {
	public sealed class FuckCommandHandler(
		ITelegramBotClient telegramBotClient
	) : ICommandHandler<FuckCommand> {
		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;

		public async Task Handle(FuckCommand command, CancellationToken cancellationToken) {
			try {
				string stdout = BrainfuckInterpreter.RunBrainfuck(
					code: command.Code
				);
				await _telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: WebUtility.HtmlEncode(stdout),
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
					cancellationToken: cancellationToken
				);
			} catch (InvalidProgramException exc) {
				await _telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: "<code>" + WebUtility.HtmlEncode(exc.Message) + "</code>",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
					cancellationToken: cancellationToken
				);
			} catch (IndexOutOfRangeException) {
				await _telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: "<code>Memory access violation</code>",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
					cancellationToken: cancellationToken
				);
			} catch (TimeoutException) {
				await _telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: "<code>Operation timed out</code>",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
					cancellationToken: cancellationToken
				);
			}
		}
	}
}
