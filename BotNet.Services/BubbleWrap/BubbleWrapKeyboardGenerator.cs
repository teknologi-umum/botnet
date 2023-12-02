using System;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotNet.Services.BubbleWrap {
	public sealed class BubbleWrapKeyboardGenerator {
		public static readonly InlineKeyboardMarkup EMPTY_KEYBOARD = BubbleWrapSheet.EmptySheet.ToKeyboardMarkup();
		private readonly IMemoryCache _memoryCache;

		public BubbleWrapKeyboardGenerator(
			IMemoryCache memoryCache
		) {
			_memoryCache = memoryCache;
		}

		public InlineKeyboardMarkup HandleCallback(int messageId, string callbackData) {
			BubbleWrapId id = new(messageId);
			BubbleWrapSheet expectedSheet = BubbleWrapSheet.ParseCallbackData(callbackData);
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

		private readonly record struct BubbleWrapId(int MessageId);
	}
}
