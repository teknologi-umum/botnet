using System;
using System.Collections.Concurrent;

namespace BotNet.Services.RateLimit.Internal {
	internal class PerUserPerChatRateLimiter : RateLimiter {
		private readonly int _actionCount;
		private readonly TimeSpan _window;

		private readonly ConcurrentDictionary<(long ChatId, long UserId), ConcurrentQueue<DateTime>> _queueByChatIdUserId = new();

		public PerUserPerChatRateLimiter(
			int actionCount,
			TimeSpan window
		) {
			_actionCount = actionCount;
			_window = window;
		}

		public override void ValidateActionRate(long chatId, long userId) {
			ConcurrentQueue<DateTime> queue = _queueByChatIdUserId.GetOrAdd(
				key: (chatId, userId),
				valueFactory: _ => new ConcurrentQueue<DateTime>()
			);
			DateTime lru = DateTime.Now;
			while (queue.TryPeek(out lru)
				&& DateTime.Now - lru > _window
				&& queue.TryDequeue(out lru)) { }
			if (queue.Count >= _actionCount) throw new RateLimitExceededException(CooldownFormatter.Format(DateTime.Now - lru));
			queue.Enqueue(DateTime.Now);
		}
	}
}
