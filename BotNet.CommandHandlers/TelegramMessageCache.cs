using BotNet.Commands;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using Microsoft.Extensions.Caching.Memory;

namespace BotNet.CommandHandlers {
	internal sealed class TelegramMessageCache(
		IMemoryCache memoryCache
	) : ITelegramMessageCache {
		private static readonly TimeSpan CACHE_TTL = TimeSpan.FromHours(1);
		private readonly IMemoryCache _memoryCache = memoryCache;

		public void Add(MessageBase message) {
			_memoryCache.Set(
				key: new Key(message.MessageId, message.Chat.Id),
				value: message,
				absoluteExpirationRelativeToNow: CACHE_TTL
			);
		}

		public MessageBase? GetOrDefault(MessageId messageId, ChatId chatId) {
			if (_memoryCache.TryGetValue<MessageBase>(
				key: new Key(messageId, chatId),
				value: out MessageBase? message
			)) {
				return message;
			} else {
				return null;
			}
		}

		public IEnumerable<MessageBase> GetThread(MessageId messageId, ChatId chatId) {
			while (GetOrDefault(messageId, chatId) is MessageBase message) {
				yield return message;
				if (message.ReplyToMessage == null) {
					yield break;
				}
				messageId = message.ReplyToMessage.MessageId;
			}
		}

		public IEnumerable<MessageBase> GetThread(MessageBase firstMessage) {
			yield return firstMessage;
			Add(firstMessage);
			if (firstMessage.ReplyToMessage is not null) {
				foreach (MessageBase reply in GetThread(firstMessage.ReplyToMessage.MessageId, firstMessage.Chat.Id)) {
					yield return reply;
				}
			}
		}

		readonly record struct Key(
			MessageId MessageId,
			ChatId ChatId
		);
	}
}
