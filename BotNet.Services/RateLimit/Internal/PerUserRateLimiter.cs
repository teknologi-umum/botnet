using System;
using System.Collections.Concurrent;

namespace BotNet.Services.RateLimit.Internal {
	internal class PerUserRateLimiter : RateLimiter {
		private readonly int _actionCount;
		private readonly TimeSpan _window;

		private readonly ConcurrentDictionary<long, ConcurrentQueue<DateTime>> _queueByUserId = new();

		public PerUserRateLimiter(
			int actionCount,
			TimeSpan window
		) {
			_actionCount = actionCount;
			_window = window;
		}

		public override void ValidateActionRate(long _, long userId) {
			ConcurrentQueue<DateTime> queue = _queueByUserId.GetOrAdd(
				key: userId,
				valueFactory: _ => new ConcurrentQueue<DateTime>()
			);
			DateTime lru = DateTime.Now;
			while (queue.TryPeek(out lru)
				&& DateTime.Now - lru > _window
				&& queue.TryDequeue(out lru)) { }
			if (queue.Count >= _actionCount) throw new RateLimitExceededException(CooldownFormatter.Format(_window - (DateTime.Now - lru)));
			queue.Enqueue(DateTime.Now);
		}
	}
}
