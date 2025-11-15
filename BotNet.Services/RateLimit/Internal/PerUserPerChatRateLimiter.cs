using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BotNet.Services.RateLimit.Internal {
	internal class PerUserPerChatRateLimiter(
		int actionCount,
		TimeSpan window
	) : RateLimiter {
		private readonly ConcurrentDictionary<(long ChatId, long UserId), ConcurrentQueue<DateTime>> _queueByChatIdUserId = new();
		private DateTime _lastCleanup = DateTime.Now;
		private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

		public override void ValidateActionRate(long chatId, long userId) {
			// Periodic cleanup of expired entries to prevent memory leak
			if (DateTime.Now - _lastCleanup > _cleanupInterval) {
				CleanupExpiredEntries();
				_lastCleanup = DateTime.Now;
			}

			ConcurrentQueue<DateTime> queue = _queueByChatIdUserId.GetOrAdd(
				key: (chatId, userId),
				valueFactory: _ => new ConcurrentQueue<DateTime>()
			);
			// ReSharper disable once RedundantAssignment
			DateTime lru = DateTime.Now;
			while (queue.TryPeek(out lru)
				&& DateTime.Now - lru > window
				&& queue.TryDequeue(out lru)) { }
			if (queue.Count >= actionCount) {
				RateLimiterMetrics.RecordRateLimitExceeded("PerUserPerChat");
				throw new RateLimitExceededException(CooldownFormatter.Format(window - (DateTime.Now - lru)));
			}
			queue.Enqueue(DateTime.Now);
		}

	private void CleanupExpiredEntries() {
		int initialCount = _queueByChatIdUserId.Count;
		DateTime cutoff = DateTime.Now - (window * 2); // Keep entries up to 2x the window
		foreach (KeyValuePair<(long ChatId, long UserId), ConcurrentQueue<DateTime>> kvp in _queueByChatIdUserId.ToArray()) {
			// Remove if queue is empty or all entries are expired
			if (kvp.Value.IsEmpty || 
			    (kvp.Value.TryPeek(out DateTime oldestEntry) && oldestEntry < cutoff)) {
				_queueByChatIdUserId.TryRemove(kvp.Key, out _);
			}
		}
		
		int entriesRemoved = initialCount - _queueByChatIdUserId.Count;
		RateLimiterMetrics.RecordCleanup("PerUserPerChat", entriesRemoved);
	}		public int GetDictionarySize() => _queueByChatIdUserId.Count;
	}
}
