using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BotNet.Services.RateLimit.Internal {
	internal class PerUserRateLimiter(
		int actionCount,
		TimeSpan window
	) : RateLimiter {
		private readonly ConcurrentDictionary<long, ConcurrentQueue<DateTime>> _queueByUserId = new();
		private DateTime _lastCleanup = DateTime.Now;
		private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

		public override void ValidateActionRate(long _, long userId) {
			// Periodic cleanup of expired entries to prevent memory leak
			if (DateTime.Now - _lastCleanup > _cleanupInterval) {
				CleanupExpiredEntries();
				_lastCleanup = DateTime.Now;
			}

			ConcurrentQueue<DateTime> queue = _queueByUserId.GetOrAdd(
				key: userId,
				valueFactory: _ => new ConcurrentQueue<DateTime>()
			);
			// ReSharper disable once RedundantAssignment
			DateTime lru = DateTime.Now;
			while (queue.TryPeek(out lru)
				&& DateTime.Now - lru > window
				&& queue.TryDequeue(out lru)) { }
			if (queue.Count >= actionCount) {
				RateLimiterMetrics.RecordRateLimitExceeded("PerUser");
				throw new RateLimitExceededException(CooldownFormatter.Format(window - (DateTime.Now - lru)));
			}
			queue.Enqueue(DateTime.Now);
		}

	private void CleanupExpiredEntries() {
		int initialCount = _queueByUserId.Count;
		DateTime cutoff = DateTime.Now - (window * 2);
		foreach (KeyValuePair<long, ConcurrentQueue<DateTime>> kvp in _queueByUserId.ToArray()) {
			if (kvp.Value.IsEmpty || 
			    (kvp.Value.TryPeek(out DateTime oldestEntry) && oldestEntry < cutoff)) {
				_queueByUserId.TryRemove(kvp.Key, out _);
			}
		}
		
		int entriesRemoved = initialCount - _queueByUserId.Count;
		RateLimiterMetrics.RecordCleanup("PerUser", entriesRemoved);
	}		public int GetDictionarySize() => _queueByUserId.Count;
	}
}
