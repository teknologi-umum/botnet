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
			BanQueue queue = _banQueuesByChat.GetOrAdd(chatId, _ => new BanQueue());

			lock (queue.Lock) {
				DateTimeOffset now = timeProvider.GetUtcNow();
				
				// Remove expired timestamps
				while (queue.NotificationTimestamps.Count > 0 && 
				       now - queue.NotificationTimestamps.Peek() > _rateWindow) {
					queue.NotificationTimestamps.Dequeue();
				}

				// Check if we can send immediate notification
				if (queue.NotificationTimestamps.Count < MaxImmediateNotifications) {
					queue.NotificationTimestamps.Enqueue(now);
					
					// Send immediate notification
					_ = SendNotificationAsync(chatId, displayName, cancellationToken);
					
					return;
				}

				// Add to pending list
				queue.PendingBans.Add(new BanRecord(displayName, now));
				
				// Schedule batch notification if not already scheduled
				if (!queue.HasScheduledNotification) {
					queue.HasScheduledNotification = true;
					DateTimeOffset oldestTimestamp = queue.NotificationTimestamps.Peek();
					TimeSpan delay = _rateWindow - (now - oldestTimestamp);
					
					_ = ScheduleBatchNotificationAsync(chatId, delay, cancellationToken);
				}
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

				List<BanRecord> bansToReport;
				lock (queue.Lock) {
					if (queue.PendingBans.Count == 0) {
						queue.HasScheduledNotification = false;
						return;
					}

					bansToReport = new List<BanRecord>(queue.PendingBans);
					queue.PendingBans.Clear();
					queue.HasScheduledNotification = false;
					
					// Add timestamp for this batch notification
					queue.NotificationTimestamps.Enqueue(timeProvider.GetUtcNow());
				}

				// Send batch notification
				string userList = string.Join("\n", 
					bansToReport.Select(b => $"• {b.DisplayName}"));
				
				string message = bansToReport.Count == 1
					? $"User {bansToReport[0].DisplayName} has been banned for posting phishing link"
					: $"The following {bansToReport.Count} users have been banned for posting phishing links:\n{userList}";

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
			public object Lock { get; } = new object();
			public Queue<DateTimeOffset> NotificationTimestamps { get; } = new();
			public List<BanRecord> PendingBans { get; } = new();
			public bool HasScheduledNotification { get; set; }
		}

		private sealed record BanRecord(string DisplayName, DateTimeOffset BannedAt);
	}
}
