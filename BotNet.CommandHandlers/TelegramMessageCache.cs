using BotNet.Commands;
using BotNet.Commands.BotUpdate.Message;
using Microsoft.Extensions.Caching.Memory;

namespace BotNet.CommandHandlers {
	internal sealed class TelegramMessageCache(
		IMemoryCache memoryCache
	) : ITelegramMessageCache {
		private static readonly TimeSpan CACHE_TTL = TimeSpan.FromHours(1);
		private readonly IMemoryCache _memoryCache = memoryCache;

		public void Set(MessageBase message) {
			_memoryCache.Set(
				key: new Key(message.MessageId, message.ChatId),
				value: message,
				absoluteExpirationRelativeToNow: CACHE_TTL
			);
		}

		public MessageBase? GetOrDefault(int messageId, long chatId) {
			if (_memoryCache.TryGetValue<MessageBase>(
				key: new Key(messageId, chatId),
				value: out MessageBase? message
			)) {
				return message;
			} else {
				return null;
			}
		}

		public IEnumerable<MessageBase> GetThread(int messageId, long chatId) {
			while (GetOrDefault(messageId, chatId) is MessageBase message) {
				yield return message;
				if (message.ReplyToMessageId == null) {
					yield break;
				}
				messageId = message.ReplyToMessageId.Value;
			}
		}

		readonly record struct Key(
			int MessageId,
			long ChatId
		);
	}
}
