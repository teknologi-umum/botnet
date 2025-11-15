using Prometheus;

namespace BotNet.CommandHandlers {
	internal static class MessageCacheMetrics {
		private static readonly Counter CacheHits = Metrics.CreateCounter(
			"botnet_message_cache_hits_total",
			"Total number of cache hits"
		);

		private static readonly Counter CacheMisses = Metrics.CreateCounter(
			"botnet_message_cache_misses_total",
			"Total number of cache misses"
		);

		private static readonly Gauge CacheSize = Metrics.CreateGauge(
			"botnet_message_cache_size",
			"Current number of items in message cache"
		);

		private static readonly Counter CacheEvictions = Metrics.CreateCounter(
			"botnet_message_cache_evictions_total",
			"Total number of cache evictions"
		);

		public static void RecordHit() => CacheHits.Inc();
		public static void RecordMiss() => CacheMisses.Inc();
		public static void RecordEviction() => CacheEvictions.Inc();
		public static void SetSize(int size) => CacheSize.Set(size);
	}
}
