﻿using System;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotNet.Services.BubbleWrap {
	public sealed class BubbleWrapKeyboardGenerator(
		IMemoryCache memoryCache
	) {
		public static readonly InlineKeyboardMarkup EMPTY_KEYBOARD = BubbleWrapSheet.EmptySheet.ToKeyboardMarkup();
		private readonly IMemoryCache _memoryCache = memoryCache;

		public InlineKeyboardMarkup HandleCallback(long chatId, int messageId, string sheetData) {
			BubbleWrapId id = new(chatId, messageId);
			BubbleWrapSheet expectedSheet = BubbleWrapSheet.ParseSheetData(sheetData);
			if (_memoryCache.TryGetValue(id, out BubbleWrapSheet? cachedSheet)) {
				cachedSheet = cachedSheet!.CombineWith(expectedSheet);
				_memoryCache.Set(
					key: id,
					value: cachedSheet,
					absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(1)
				);
				return cachedSheet.ToKeyboardMarkup();
			} else {
				_memoryCache.Set(
					key: id,
					value: expectedSheet,
					absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(1)
				);
				return expectedSheet.ToKeyboardMarkup();
			}
		}

		private readonly record struct BubbleWrapId(
			long ChatId,
			int MessageId
		);
	}
}
