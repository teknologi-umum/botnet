using System;
using System.Collections.Concurrent;

namespace BotNet.Services.RateLimit.Internal {
	internal class PerUserPerDayRateLimiter : RateLimiter {
		private readonly int _actionCount;
		private readonly TimeSpan _timeZoneOffset;

		private readonly ConcurrentDictionary<long, ConcurrentQueue<DateTime>> _queueByUserId = new();

		public PerUserPerDayRateLimiter(
			int actionCount,
			TimeSpan timeZoneOffset
		) {
			_actionCount = actionCount;
			_timeZoneOffset = timeZoneOffset;
		}

		public override void ValidateActionRate(long _, long userId) {
			ConcurrentQueue<DateTime> queue = _queueByUserId.GetOrAdd(
				key: userId,
				valueFactory: _ => new ConcurrentQueue<DateTime>()
			);
			DateTime localDate = DateTime.UtcNow.Date + _timeZoneOffset;
			while (queue.TryPeek(out DateTime lru)
				&& lru != localDate
				&& queue.TryDequeue(out lru)) { }
			if (queue.Count >= _actionCount) throw new RateLimitExceededException("besok");
			queue.Enqueue(localDate);
		}
	}
}
