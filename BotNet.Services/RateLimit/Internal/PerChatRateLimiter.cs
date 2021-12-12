using System;
using System.Collections.Concurrent;

namespace BotNet.Services.RateLimit.Internal {
	internal class PerChatRateLimiter : RateLimiter {
		private readonly int _actionCount;
		private readonly TimeSpan _window;

		private readonly ConcurrentDictionary<long, ConcurrentQueue<DateTime>> _queueByChatId = new();

		public PerChatRateLimiter(
			int actionCount,
			TimeSpan window
		) {
			_actionCount = actionCount;
			_window = window;
		}

		public override void ValidateActionRate(long chatId, long _) {
			ConcurrentQueue<DateTime> queue = _queueByChatId.GetOrAdd(
				key: chatId,
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
