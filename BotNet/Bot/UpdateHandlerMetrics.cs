using System;
using Prometheus;

namespace BotNet.Bot {
	internal static class UpdateHandlerMetrics {
		private static readonly Counter UpdatesReceived = Metrics.CreateCounter(
			"botnet_updates_received_total",
			"Total number of Telegram updates received",
			new CounterConfiguration {
				LabelNames = new[] { "update_type" }
			}
		);

		private static readonly Counter UpdatesProcessed = Metrics.CreateCounter(
			"botnet_updates_processed_total",
			"Total number of updates processed successfully",
			new CounterConfiguration {
				LabelNames = new[] { "update_type" }
			}
		);

		private static readonly Counter UpdateErrors = Metrics.CreateCounter(
			"botnet_update_errors_total",
			"Total number of update processing errors",
			new CounterConfiguration {
				LabelNames = new[] { "update_type", "error_type" }
			}
		);

		private static readonly Histogram UpdateProcessingDuration = Metrics.CreateHistogram(
			"botnet_update_processing_duration_seconds",
			"Time spent processing Telegram updates",
			new HistogramConfiguration {
				LabelNames = new[] { "update_type" },
				Buckets = Histogram.ExponentialBuckets(0.001, 2, 12) // 1ms to ~4s
			}
		);

		public static void RecordReceived(string updateType) {
			UpdatesReceived.WithLabels(updateType).Inc();
		}

		public static void RecordProcessed(string updateType) {
			UpdatesProcessed.WithLabels(updateType).Inc();
		}

		public static void RecordError(string updateType, string errorType) {
			UpdateErrors.WithLabels(updateType, errorType).Inc();
		}

		public static IDisposable MeasureProcessingDuration(string updateType) {
			return UpdateProcessingDuration.WithLabels(updateType).NewTimer();
		}
	}
}
