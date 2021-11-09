using System;

namespace BotNet.Services.RateLimit {
	public class RateLimitExceededException : Exception {
		public string Cooldown { get; }

		public RateLimitExceededException(string cooldown) {
			Cooldown = cooldown;
		}
	}
}
