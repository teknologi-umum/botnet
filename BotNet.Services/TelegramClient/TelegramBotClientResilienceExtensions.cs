using System;
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
					return await telegramBotClient.EditMessageTextAsync(
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
			throw new ApiRequestException("Text could not be parsed.");
		}
	}
}
