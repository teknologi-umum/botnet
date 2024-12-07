using System;

namespace BotNet.Services.RateLimit {
	public class RateLimitExceededException(
		string cooldown
	) : Exception {
		public string Cooldown { get; } = cooldown;
	}
}
