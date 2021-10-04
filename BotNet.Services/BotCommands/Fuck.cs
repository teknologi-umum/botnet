using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Brainfuck;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Fuck {
		public static async Task HandleFuckAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
			if (message.Entities is { Length: 1 } entities
				&& entities[0] is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					try {
						string stdout = BrainfuckInterpreter.RunBrainfuck(commandArgument);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: WebUtility.HtmlEncode(stdout),
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} catch (InvalidProgramException exc) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>" + WebUtility.HtmlEncode(exc.Message) + "</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} catch (IndexOutOfRangeException) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>Memory access violation</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else if (message.ReplyToMessage?.Text is string repliedToMessage) {
					try {
						string stdout = BrainfuckInterpreter.RunBrainfuck(repliedToMessage);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: WebUtility.HtmlEncode(stdout),
							parseMode: ParseMode.Html,
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
					} catch (InvalidProgramException exc) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>" + WebUtility.HtmlEncode(exc.Message) + "</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
					} catch (IndexOutOfRangeException) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>Memory access violation</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} catch (TimeoutException) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>Operation timed out</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: "Untuk menjalankan program brainfuck, silahkan ketik /fuck diikuti kode program.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
		}
	}
}
