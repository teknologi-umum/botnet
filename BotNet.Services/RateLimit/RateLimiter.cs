using System;
using BotNet.Services.RateLimit.Internal;

namespace BotNet.Services.RateLimit {
	/// <summary>
	/// Note: This rate limiter is for monolith app and isn't ready to be used in Orleans
	/// </summary>
	public abstract class RateLimiter {
		public static RateLimiter PerChat(int actionCount, TimeSpan window) => new PerChatRateLimiter(actionCount, window);
		public static RateLimiter PerUser(int actionCount, TimeSpan window) => new PerUserRateLimiter(actionCount, window);
		public static RateLimiter PerUserPerChat(int actionCount, TimeSpan window) => new PerUserPerChatRateLimiter(actionCount, window);
		public static RateLimiter PerUserPerDay(int actionCount) => new PerUserPerDayRateLimiter(actionCount, TimeSpan.FromHours(7));
		public static RateLimiter PerUserPerChatPerDay(int actionCount) => new PerUserPerChatPerDayRateLimiter(actionCount, TimeSpan.FromHours(7));

		public abstract void ValidateActionRate(long chatId, long userId);
	}
}
