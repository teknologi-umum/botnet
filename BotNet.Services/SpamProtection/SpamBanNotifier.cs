using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.SpamProtection {
	public sealed class SpamBanNotifier(
		ITelegramBotClient telegramBotClient,
		TimeProvider timeProvider,
		ILogger<SpamBanNotifier> logger
	) {
		private readonly ConcurrentDictionary<long, BanQueue> _banQueuesByChat = new();
		private readonly TimeSpan _rateWindow = TimeSpan.FromMinutes(10);
		private const int MaxImmediateNotifications = 3;

		public async Task NotifyBanAsync(
			long chatId,
			string displayName,
			CancellationToken cancellationToken
		) {
			BanQueue queue = _banQueuesByChat.GetOrAdd(chatId, _ => new BanQueue(_rateWindow, MaxImmediateNotifications));

			BanQueue.NotificationDecision decision = queue.RecordBan(displayName, timeProvider.GetUtcNow());

			if (decision.ShouldSendImmediate) {
				_ = SendNotificationAsync(chatId, displayName, cancellationToken);
			} else if (decision.ShouldScheduleBatch) {
				_ = ScheduleBatchNotificationAsync(chatId, decision.BatchDelay!.Value, cancellationToken);
			}
		}

		private async Task SendNotificationAsync(
			long chatId,
			string displayName,
			CancellationToken cancellationToken
		) {
			try {
				await telegramBotClient.SendMessage(
					chatId: chatId,
					text: $"User {displayName} has been banned for posting phishing link",
					parseMode: ParseMode.Html,
					cancellationToken: cancellationToken
				);
			} catch (Exception exc) {
				logger.LogError(
					exc,
					"Failed to send ban notification for user {DisplayName} in chat {ChatId}",
					displayName,
					chatId
				);
			}
		}

		private async Task ScheduleBatchNotificationAsync(
			long chatId,
			TimeSpan delay,
			CancellationToken cancellationToken
		) {
			try {
				// Create a task that completes after the delay, respecting the TimeProvider
				TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
				ITimer timer = timeProvider.CreateTimer(_ => tcs.TrySetResult(true), null, delay, Timeout.InfiniteTimeSpan);
				
				using CancellationTokenRegistration registration = cancellationToken.Register(() => tcs.TrySetCanceled());
				
				try {
					await tcs.Task;
				} finally {
					await timer.DisposeAsync();
				}

				if (!_banQueuesByChat.TryGetValue(chatId, out BanQueue? queue)) {
					return;
				}

				BanQueue.BatchResult batchResult = queue.GetBatchAndMarkAsSent(timeProvider.GetUtcNow());

				if (batchResult.BansToReport.Count == 0) {
					return;
				}

				// Send batch notification
				string userList = string.Join("\n", 
					batchResult.BansToReport.Select(b => $"• {b.DisplayName}"));
				
				string message = batchResult.BansToReport.Count == 1
					? $"User {batchResult.BansToReport[0].DisplayName} has been banned for posting phishing link"
					: $"The following {batchResult.BansToReport.Count} users have been banned for posting phishing links:\n{userList}";

				await telegramBotClient.SendMessage(
					chatId: chatId,
					text: message,
					parseMode: ParseMode.Html,
					cancellationToken: cancellationToken
				);
			} catch (OperationCanceledException) {
				// Ignore cancellation
			} catch (Exception exc) {
				logger.LogError(
					exc,
					"Failed to send batch ban notification in chat {ChatId}",
					chatId
				);
			}
		}

		private sealed class BanQueue {
			private readonly object _lock = new object();
			private readonly Queue<DateTimeOffset> _notificationTimestamps = new();
			private readonly List<BanRecord> _pendingBans = new();
			private readonly TimeSpan _rateWindow;
			private readonly int _maxImmediateNotifications;
			private bool _hasScheduledNotification;

			public BanQueue(TimeSpan rateWindow, int maxImmediateNotifications) {
				_rateWindow = rateWindow;
				_maxImmediateNotifications = maxImmediateNotifications;
			}

			public NotificationDecision RecordBan(string displayName, DateTimeOffset now) {
				lock (_lock) {
					// Remove expired timestamps
					RemoveExpiredTimestamps(now);

					// Check if we can send immediate notification
					if (_notificationTimestamps.Count < _maxImmediateNotifications) {
						_notificationTimestamps.Enqueue(now);
						return new NotificationDecision(
							ShouldSendImmediate: true,
							ShouldScheduleBatch: false,
							BatchDelay: null
						);
					}

					// Add to pending list
					_pendingBans.Add(new BanRecord(displayName, now));

					// Schedule batch notification if not already scheduled
					if (!_hasScheduledNotification) {
						_hasScheduledNotification = true;
						DateTimeOffset oldestTimestamp = _notificationTimestamps.Peek();
						TimeSpan delay = _rateWindow - (now - oldestTimestamp);
						
						return new NotificationDecision(
							ShouldSendImmediate: false,
							ShouldScheduleBatch: true,
							BatchDelay: delay
						);
					}

					return new NotificationDecision(
						ShouldSendImmediate: false,
						ShouldScheduleBatch: false,
						BatchDelay: null
					);
				}
			}

			public BatchResult GetBatchAndMarkAsSent(DateTimeOffset now) {
				lock (_lock) {
					if (_pendingBans.Count == 0) {
						_hasScheduledNotification = false;
						return new BatchResult(new List<BanRecord>());
					}

					List<BanRecord> bansToReport = new List<BanRecord>(_pendingBans);
					_pendingBans.Clear();
					_hasScheduledNotification = false;
					
					// Add timestamp for this batch notification
					_notificationTimestamps.Enqueue(now);

					return new BatchResult(bansToReport);
				}
			}

			private void RemoveExpiredTimestamps(DateTimeOffset now) {
				while (_notificationTimestamps.Count > 0 && 
				       now - _notificationTimestamps.Peek() > _rateWindow) {
					_notificationTimestamps.Dequeue();
				}
			}

			public sealed record NotificationDecision(
				bool ShouldSendImmediate,
				bool ShouldScheduleBatch,
				TimeSpan? BatchDelay
			);

			public sealed record BatchResult(List<BanRecord> BansToReport);
		}

		private sealed record BanRecord(string DisplayName, DateTimeOffset BannedAt);
	}
}
