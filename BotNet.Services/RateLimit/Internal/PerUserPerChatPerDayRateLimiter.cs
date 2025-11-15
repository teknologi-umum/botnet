using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BotNet.Services.RateLimit.Internal {
	internal class PerUserPerChatPerDayRateLimiter : RateLimiter {
		private readonly int _actionCount;
		private readonly TimeSpan _timeZoneOffset;

		private readonly ConcurrentDictionary<(long ChatId, long UserId), ConcurrentQueue<DateTime>> _queueByUserId = new();
		private DateTime _lastCleanup = DateTime.UtcNow;
		private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6);

		public PerUserPerChatPerDayRateLimiter(
			int actionCount,
			TimeSpan timeZoneOffset
		) {
			_actionCount = actionCount;
			_timeZoneOffset = timeZoneOffset;
		}

		public override void ValidateActionRate(long chatId, long userId) {
			// Periodic cleanup of expired entries to prevent memory leak
			if (DateTime.UtcNow - _lastCleanup > _cleanupInterval) {
				CleanupExpiredEntries();
				_lastCleanup = DateTime.UtcNow;
			}

			ConcurrentQueue<DateTime> queue = _queueByUserId.GetOrAdd(
				key: (chatId, userId),
				valueFactory: _ => new ConcurrentQueue<DateTime>()
			);
			DateTime localDate = DateTime.UtcNow.Date + _timeZoneOffset;
			while (queue.TryPeek(out DateTime lru)
				&& lru != localDate
				&& queue.TryDequeue(out lru)) { }
			if (queue.Count >= _actionCount) {
				RateLimiterMetrics.RecordRateLimitExceeded("PerUserPerChatPerDay");
				throw new RateLimitExceededException("besok");
			}
			queue.Enqueue(localDate);
		}

	private void CleanupExpiredEntries() {
		int initialCount = _queueByUserId.Count;
		DateTime yesterday = (DateTime.UtcNow.Date + _timeZoneOffset).AddDays(-1);
		foreach (KeyValuePair<(long ChatId, long UserId), ConcurrentQueue<DateTime>> kvp in _queueByUserId.ToArray()) {
			// Remove if queue is empty or contains only old dates
			if (kvp.Value.IsEmpty || 
			    (kvp.Value.TryPeek(out DateTime oldestEntry) && oldestEntry < yesterday)) {
				_queueByUserId.TryRemove(kvp.Key, out _);
			}
		}
		
		int entriesRemoved = initialCount - _queueByUserId.Count;
		RateLimiterMetrics.RecordCleanup("PerUserPerChatPerDay", entriesRemoved);
	}		public int GetDictionarySize() => _queueByUserId.Count;
	}
}
