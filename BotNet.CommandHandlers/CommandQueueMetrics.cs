using Prometheus;

namespace BotNet.CommandHandlers {
	public static class CommandQueueMetrics {
		private static readonly Gauge QueueDepth = Metrics.CreateGauge(
			"botnet_command_queue_depth",
			"Current number of commands waiting in the queue"
		);

		private static readonly Counter CommandsEnqueued = Metrics.CreateCounter(
			"botnet_command_queue_enqueued_total",
			"Total number of commands enqueued"
		);

		private static readonly Counter CommandsProcessed = Metrics.CreateCounter(
			"botnet_command_queue_processed_total",
			"Total number of commands processed"
		);

		private static readonly Counter CommandsDropped = Metrics.CreateCounter(
			"botnet_command_queue_dropped_total",
			"Total number of commands dropped due to full queue"
		);

		private static readonly Histogram ProcessingDuration = Metrics.CreateHistogram(
			"botnet_command_processing_duration_seconds",
			"Time spent processing each command",
			new HistogramConfiguration {
				Buckets = Histogram.ExponentialBuckets(0.01, 2, 10) // 10ms to 5.12s
			}
		);

		private static readonly Histogram QueueWaitTime = Metrics.CreateHistogram(
			"botnet_command_queue_wait_time_seconds",
			"Time commands spend waiting in queue",
			new HistogramConfiguration {
				Buckets = Histogram.ExponentialBuckets(0.001, 2, 10) // 1ms to 512ms
			}
		);

		public static void RecordEnqueued() => CommandsEnqueued.Inc();
		public static void RecordDropped() => CommandsDropped.Inc();
		public static void RecordProcessed() => CommandsProcessed.Inc();
		public static void SetQueueDepth(int depth) => QueueDepth.Set(depth);
		public static IDisposable MeasureProcessingDuration() => ProcessingDuration.NewTimer();
		public static void RecordQueueWaitTime(double seconds) => QueueWaitTime.Observe(seconds);
	}
}
