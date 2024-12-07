using System;
using System.Collections.Concurrent;

namespace BotNet.Services.RateLimit.Internal {
	internal class PerUserRateLimiter(
		int actionCount,
		TimeSpan window
	) : RateLimiter {
		private readonly ConcurrentDictionary<long, ConcurrentQueue<DateTime>> _queueByUserId = new();

		public override void ValidateActionRate(long _, long userId) {
			ConcurrentQueue<DateTime> queue = _queueByUserId.GetOrAdd(
				key: userId,
				valueFactory: _ => new ConcurrentQueue<DateTime>()
			);
			// ReSharper disable once RedundantAssignment
			DateTime lru = DateTime.Now;
			while (queue.TryPeek(out lru)
				&& DateTime.Now - lru > window
				&& queue.TryDequeue(out lru)) { }
			if (queue.Count >= actionCount) throw new RateLimitExceededException(CooldownFormatter.Format(window - (DateTime.Now - lru)));
			queue.Enqueue(DateTime.Now);
		}
	}
}
