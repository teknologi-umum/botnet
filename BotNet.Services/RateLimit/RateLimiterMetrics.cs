using Prometheus;

namespace BotNet.Services.RateLimit {
	/// <summary>
	/// Prometheus metrics for rate limiter monitoring
	/// </summary>
	public static class RateLimiterMetrics {
		private static readonly Gauge RateLimiterDictionarySize = Metrics.CreateGauge(
			"botnet_rate_limiter_dictionary_size",
			"Number of entries in rate limiter dictionaries",
			new GaugeConfiguration {
				LabelNames = new[] { "limiter_type", "handler" }
			}
		);

		private static readonly Counter RateLimitExceeded = Metrics.CreateCounter(
			"botnet_rate_limit_exceeded_total",
			"Total number of rate limit violations",
			new CounterConfiguration {
				LabelNames = new[] { "handler" }
			}
		);

		private static readonly Counter RateLimiterCleanups = Metrics.CreateCounter(
			"botnet_rate_limiter_cleanups_total",
			"Total number of rate limiter cleanup operations",
			new CounterConfiguration {
				LabelNames = new[] { "limiter_type" }
			}
		);

		private static readonly Gauge RateLimiterEntriesRemoved = Metrics.CreateGauge(
			"botnet_rate_limiter_entries_removed",
			"Number of entries removed during last cleanup",
			new GaugeConfiguration {
				LabelNames = new[] { "limiter_type" }
			}
		);

		public static void RecordDictionarySize(string limiterType, string handler, int size) {
			RateLimiterDictionarySize
				.WithLabels(limiterType, handler)
				.Set(size);
		}

		public static void RecordRateLimitExceeded(string handler) {
			RateLimitExceeded
				.WithLabels(handler)
				.Inc();
		}

		public static void RecordCleanup(string limiterType, int entriesRemoved) {
			RateLimiterCleanups
				.WithLabels(limiterType)
				.Inc();
			
			RateLimiterEntriesRemoved
				.WithLabels(limiterType)
				.Set(entriesRemoved);
		}
	}
}
