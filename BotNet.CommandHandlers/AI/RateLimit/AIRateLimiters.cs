using BotNet.Services.RateLimit;

namespace BotNet.CommandHandlers.AI.RateLimit {
	internal static class AiRateLimiters {
		internal static readonly RateLimiter GroupChatRateLimiter = RateLimiter.PerUserPerChat(4, TimeSpan.FromMinutes(60));
	}
}
