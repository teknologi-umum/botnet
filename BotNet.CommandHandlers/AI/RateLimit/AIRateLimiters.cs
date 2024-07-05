using BotNet.Services.RateLimit;

namespace BotNet.CommandHandlers.AI.RateLimit {
	internal static class AIRateLimiters {
		internal static readonly RateLimiter GROUP_CHAT_RATE_LIMITER = RateLimiter.PerUserPerChat(4, TimeSpan.FromMinutes(60));
	}
}
