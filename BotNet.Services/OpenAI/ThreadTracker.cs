using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace BotNet.Services.OpenAI {
	public sealed class ThreadTracker {
		private readonly IMemoryCache _memoryCache;

		public ThreadTracker(
			IMemoryCache memoryCache
		) {
			_memoryCache = memoryCache;
		}

		public void TrackMessage(
			long messageId,
			string sender,
			string text,
			long? replyToMessageId
		) {
			_memoryCache.Set(
				key: new MessageId(messageId),
				value: new Message(
					Sender: sender,
					Text: text,
					ReplyToMessageId: replyToMessageId.HasValue
						? new(replyToMessageId.Value)
						: null
				),
				absoluteExpirationRelativeToNow: TimeSpan.FromHours(1)
			);
		}

		public IEnumerable<(string Sender, string Text)> GetThread(
			long messageId,
			int maxLines
		) {
			while (_memoryCache.TryGetValue<Message>(
				key: new MessageId(messageId),
				value: out Message? message
			) && message != null && maxLines-- > 0) {
				yield return (
					Sender: message.Sender,
					Text: message.Text
				);

				if (message.ReplyToMessageId == null) {
					yield break;
				}

				messageId = message.ReplyToMessageId.Value.Value;
			}
		}

		private readonly record struct MessageId(long Value);
		private sealed record Message(
			string Sender,
			string Text,
			MessageId? ReplyToMessageId
		);
	}
}
