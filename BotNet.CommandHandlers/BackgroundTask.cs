using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BotNet.CommandHandlers {
	/// <summary>
	/// Helper for running fire-and-forget background tasks with consistent exception handling
	/// </summary>
	internal static class BackgroundTask {
		/// <summary>
		/// Runs a task in the background with automatic exception handling and logging
		/// </summary>
		/// <param name="taskFunc">The async task to run</param>
		/// <param name="logger">Logger for error reporting</param>
		public static void Run(Func<Task> taskFunc, ILogger logger) {
			BackgroundTaskMetrics.RecordStarted();
			
			Task task = Task.Run(async () => {
				using (BackgroundTaskMetrics.MeasureDuration()) {
					try {
						await taskFunc();
						BackgroundTaskMetrics.RecordCompleted();
					} catch (OperationCanceledException) {
						// Graceful shutdown - don't log
						BackgroundTaskMetrics.RecordCancelled();
					} catch (Exception ex) {
						logger.LogError(ex, "Background task failed: {Message}", ex.Message);
						BackgroundTaskMetrics.RecordFailed();
					}
				}
			});
		}
	}
}
