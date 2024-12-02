using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.MarkdownV2;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotNet.Services.TelegramClient {
	public static class TelegramBotClientResilienceExtensions {
		public static async Task<Message> SendTextMessageAsync(
			this ITelegramBotClient telegramBotClient,
			ChatId chatId,
			string text,
			ParseMode[] parseModes,
			int? replyToMessageId = null,
			InlineKeyboardMarkup? replyMarkup = null,
			CancellationToken cancellationToken = default
		) {
			if (parseModes.Length == 0) throw new ArgumentException("At least one parse mode must be provided.", nameof(parseModes));

			foreach (ParseMode parseMode in parseModes) {
				try {
					return await telegramBotClient.SendMessage(
						chatId: chatId,
						text: parseMode == ParseMode.MarkdownV2
							? MarkdownV2Sanitizer.Sanitize(text)
							: text,
						parseMode: parseMode,
						replyParameters: replyToMessageId.HasValue
							? new ReplyParameters {
								MessageId = replyToMessageId.Value
							}
							: null,
						replyMarkup: replyMarkup,
						cancellationToken: cancellationToken
					);
				} catch (ApiRequestException) {
					continue;
				}
			}

			// Last resort: escape everything
			return await telegramBotClient.SendMessage(
				chatId: chatId,
				text: WebUtility.HtmlEncode(text),
				parseMode: ParseMode.Html,
				replyParameters: replyToMessageId.HasValue
					? new ReplyParameters {
						MessageId = replyToMessageId.Value
					}
					: null,
				replyMarkup: replyMarkup,
				cancellationToken: cancellationToken
			);
		}

		public static async Task<Message> EditMessageTextAsync(
			this ITelegramBotClient telegramBotClient,
			ChatId chatId,
			int messageId,
			string text,
			ParseMode[] parseModes,
			InlineKeyboardMarkup? replyMarkup = null,
			CancellationToken cancellationToken = default
		) {
			if (parseModes.Length == 0) throw new ArgumentException("At least one parse mode must be provided.", nameof(parseModes));

			foreach (ParseMode parseMode in parseModes) {
				try {
					return await telegramBotClient.EditMessageText(
						chatId: chatId,
						messageId: messageId,
						text: parseMode == ParseMode.MarkdownV2
							? MarkdownV2Sanitizer.Sanitize(text)
							: text,
						parseMode: parseMode,
						replyMarkup: replyMarkup,
						cancellationToken: cancellationToken
					);
				} catch (ApiRequestException) {
					continue;
				}
			}
			
			// Last resort: escape everything
			return await telegramBotClient.EditMessageText(
				chatId: chatId,
				messageId: messageId,
				text: WebUtility.HtmlEncode(text),
				parseMode: ParseMode.Html,
				replyMarkup: replyMarkup,
				cancellationToken: cancellationToken
			);
		}
	}
}
