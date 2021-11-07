using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class OpenAI {
		public static async Task ExplainAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					try {
						string result = await serviceProvider.GetRequiredService<OpenAIClient>().DavinciCodexAutocompleteAsync(commandArgument + "\n\n\"\"\"\nHere's what the above code is doing:\n1. ", new[] { "\"\"\"" }, 3, cancellationToken);
						result = "1. " + result;
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>{WebUtility.HtmlEncode(result)}</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} catch (OperationCanceledException) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>Timeout exceeded.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else if (message.ReplyToMessage?.Text is string repliedToMessage) {
					try {
						string result = await serviceProvider.GetRequiredService<OpenAIClient>().DavinciCodexAutocompleteAsync(repliedToMessage + "\n\n\"\"\"\nHere's what the above code is doing:\n1. ", new[] { "\"\"\"" }, 3, cancellationToken);
						result = "1. " + result;
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>{WebUtility.HtmlEncode(result)}</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
					} catch (OperationCanceledException) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>Timeout exceeded.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: $"Untuk explain code, silahkan ketik /explain diikuti code.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
		}

		public static async Task AskHelpAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					try {
						string story = $"Berikut ini adalah sebuah percakapan antara seorang manusia bernama {message.From!.FirstName}{message.From.LastName?.Let(ln => " " + ln)} dengan sebuah bot asisten. "
							+ "Bot ini sangat ramah, membantu, kreatif, dan cerdas.\n\n"
							+ "Manusia: Halo, apa kabar?\n"
							+ "TeknumBot: Saya bot yang diciptakan oleh TEKNUM. Apakah ada yang bisa saya bantu?\n\n"
							+ $"Manusia: {commandArgument}\n"
							+ "TeknumBot: ";
						string result = await serviceProvider.GetRequiredService<OpenAIClient>().DavinciCodexAutocompleteAsync(
							source: story,
							stop: new[] { "Manusia:" },
							maxRecursion: 0,
							cancellationToken: cancellationToken
						);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: WebUtility.HtmlEncode(result),
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} catch (OperationCanceledException) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>Timeout exceeded.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else if (message.ReplyToMessage?.Text is string repliedToMessage) {
					try {
						string story = $"Berikut ini adalah sebuah percakapan antara seorang manusia bernama {message.From!.FirstName}{message.From.LastName?.Let(ln => " " + ln)} dengan sebuah bot asisten. "
							+ "Bot ini sangat ramah, membantu, kreatif, dan cerdas.\n\n"
							+ "Manusia: Halo, apa kabar?\n"
							+ "TeknumBot: Saya bot yang diciptakan oleh TEKNUM. Apakah ada yang bisa saya bantu?\n\n"
							+ $"Manusia: {repliedToMessage}\n"
							+ "TeknumBot: ";
						string result = await serviceProvider.GetRequiredService<OpenAIClient>().DavinciCodexAutocompleteAsync(
							source: story,
							stop: new[] { "Manusia:" },
							maxRecursion: 0,
							cancellationToken: cancellationToken
						);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: WebUtility.HtmlEncode(result),
							parseMode: ParseMode.Html,
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
					} catch (OperationCanceledException) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>Timeout exceeded.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: $"Untuk bertanya, silahkan ketik /ask diikuti pertanyaan.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
		}
	}
}
