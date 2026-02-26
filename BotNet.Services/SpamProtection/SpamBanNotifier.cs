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
				DateTime now = DateTime.Now;
				
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
					DateTime oldestTimestamp = queue.NotificationTimestamps.Peek();
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
				await Task.Delay(delay, cancellationToken);

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
					queue.NotificationTimestamps.Enqueue(DateTime.Now);
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
			public Queue<DateTime> NotificationTimestamps { get; } = new();
			public List<BanRecord> PendingBans { get; } = new();
			public bool HasScheduledNotification { get; set; }
		}

		private sealed record BanRecord(string DisplayName, DateTime BannedAt);
	}
}
