using System;
using System.Collections.Concurrent;

namespace BotNet.Services.RateLimit.Internal {
	internal class PerUserPerChatPerDayRateLimiter : RateLimiter {
		private readonly int _actionCount;
		private readonly TimeSpan _timeZoneOffset;

		private readonly ConcurrentDictionary<(long ChatId, long UserId), ConcurrentQueue<DateTime>> _queueByUserId = new();

		public PerUserPerChatPerDayRateLimiter(
			int actionCount,
			TimeSpan timeZoneOffset
		) {
			_actionCount = actionCount;
			_timeZoneOffset = timeZoneOffset;
		}

		public override void ValidateActionRate(long chatId, long userId) {
			ConcurrentQueue<DateTime> queue = _queueByUserId.GetOrAdd(
				key: (chatId, userId),
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
