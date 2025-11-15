using Prometheus;

namespace BotNet.CommandHandlers {
	public static class CommandMetrics {
		private static readonly Counter CommandInvocations = Metrics.CreateCounter(
			"botnet_command_invocations_total",
			"Total number of command invocations",
			new CounterConfiguration {
				LabelNames = new[] { "command_type", "sender_type", "chat_type" }
			}
		);

		private static readonly Counter CommandSuccesses = Metrics.CreateCounter(
			"botnet_command_successes_total",
			"Total number of successful command executions",
			new CounterConfiguration {
				LabelNames = new[] { "command_type" }
			}
		);

		private static readonly Counter CommandFailures = Metrics.CreateCounter(
			"botnet_command_failures_total",
			"Total number of failed command executions",
			new CounterConfiguration {
				LabelNames = new[] { "command_type", "error_type" }
			}
		);

		private static readonly Histogram CommandDuration = Metrics.CreateHistogram(
			"botnet_command_duration_seconds",
			"Time spent executing commands",
			new HistogramConfiguration {
				LabelNames = new[] { "command_type" },
				Buckets = Histogram.ExponentialBuckets(0.01, 2, 10) // 10ms to 5.12s
			}
		);

		public static void RecordInvocation(string commandType, string senderType, string chatType) {
			CommandInvocations.WithLabels(commandType, senderType, chatType).Inc();
		}

		public static void RecordSuccess(string commandType) {
			CommandSuccesses.WithLabels(commandType).Inc();
		}

		public static void RecordFailure(string commandType, string errorType) {
			CommandFailures.WithLabels(commandType, errorType).Inc();
		}

		public static IDisposable MeasureDuration(string commandType) {
			return CommandDuration.WithLabels(commandType).NewTimer();
		}
	}
}
