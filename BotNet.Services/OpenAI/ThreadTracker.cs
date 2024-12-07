using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace BotNet.Services.OpenAI {
	public sealed class ThreadTracker(
		IMemoryCache memoryCache
	) {
		public void TrackMessage(
			long messageId,
			string sender,
			string? text,
			string? imageBase64,
			long? replyToMessageId
		) {
			memoryCache.Set(
				key: new MessageId(messageId),
				value: new Message(
					Sender: sender,
					Text: text,
					ImageBase64: imageBase64,
					ReplyToMessageId: replyToMessageId.HasValue
						? new(replyToMessageId.Value)
						: null
				),
				absoluteExpirationRelativeToNow: TimeSpan.FromHours(1)
			);
		}

		public IEnumerable<(string Sender, string? Text, string? ImageBase64)> GetThread(
			long messageId,
			int maxLines
		) {
			bool firstLine = true;
			while (memoryCache.TryGetValue(
				key: new MessageId(messageId),
				value: out Message? message
			) && message != null && maxLines-- > 0) {
				if (firstLine) {
					yield return (
						Sender: message.Sender,
						Text: message.Text,
						ImageBase64: message.ImageBase64
					);
					firstLine = false;
				} else {
					yield return (
						Sender: message.Sender,
						Text: message.Text,
						ImageBase64: null // Strip images from the rest of thread
					);
				}

				if (message.ReplyToMessageId == null) {
					yield break;
				}

				messageId = message.ReplyToMessageId.Value.Value;
			}
		}

		private readonly record struct MessageId(long Value);
		private sealed record Message(
			string Sender,
			string? Text,
			string? ImageBase64,
			MessageId? ReplyToMessageId
		);
	}
}
