using Prometheus;

namespace BotNet.CommandHandlers {
	internal static class BackgroundTaskMetrics {
		private static readonly Gauge ActiveTasks = Metrics.CreateGauge(
			"botnet_background_tasks_active",
			"Current number of active background tasks"
		);

		private static readonly Counter TasksStarted = Metrics.CreateCounter(
			"botnet_background_tasks_started_total",
			"Total number of background tasks started"
		);

		private static readonly Counter TasksCompleted = Metrics.CreateCounter(
			"botnet_background_tasks_completed_total",
			"Total number of background tasks completed successfully"
		);

		private static readonly Counter TasksFailed = Metrics.CreateCounter(
			"botnet_background_tasks_failed_total",
			"Total number of background tasks that failed with exceptions"
		);

		private static readonly Counter TasksCancelled = Metrics.CreateCounter(
			"botnet_background_tasks_cancelled_total",
			"Total number of background tasks cancelled"
		);

		private static readonly Histogram TaskDuration = Metrics.CreateHistogram(
			"botnet_background_task_duration_seconds",
			"Time spent executing background tasks",
			new HistogramConfiguration {
				Buckets = Histogram.ExponentialBuckets(0.1, 2, 12) // 100ms to ~409s
			}
		);

		public static void RecordStarted() {
			ActiveTasks.Inc();
			TasksStarted.Inc();
		}

		public static void RecordCompleted() {
			ActiveTasks.Dec();
			TasksCompleted.Inc();
		}

		public static void RecordFailed() {
			ActiveTasks.Dec();
			TasksFailed.Inc();
		}

		public static void RecordCancelled() {
			ActiveTasks.Dec();
			TasksCancelled.Inc();
		}

		public static IDisposable MeasureDuration() => TaskDuration.NewTimer();
	}
}
