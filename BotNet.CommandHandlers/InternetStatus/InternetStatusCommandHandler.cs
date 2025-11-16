using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Commands.InternetStatus;
using BotNet.Services.RateLimit;
using BotNet.Services.StatusPage;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.InternetStatus {
	public sealed class InternetStatusCommandHandler(
		ITelegramBotClient telegramBotClient,
		StatusPageClient statusPageClient,
		ILogger<InternetStatusCommandHandler> logger
	) : ICommandHandler<InternetStatusCommand> {
		private static readonly RateLimiter RateLimiter = RateLimiter.PerChat(1, TimeSpan.FromMinutes(2));

		public async Task Handle(InternetStatusCommand command, CancellationToken cancellationToken) {
			try {
				RateLimiter.ValidateActionRate(
					chatId: command.Chat.Id,
					userId: command.Sender.Id
				);
			} catch (RateLimitExceededException exc) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"<code>Coba lagi {exc.Cooldown}</code>",
					parseMode: ParseMode.Html,
					cancellationToken: cancellationToken
				);
				return;
			}

			// Send "checking..." message
			Telegram.Bot.Types.Message statusMessage = await telegramBotClient.SendMessage(
				chatId: command.Chat.Id,
				text: "🔄 Checking status of major internet services...",
				cancellationToken: cancellationToken
			);

			try {
				List<ServiceStatus> serviceStatuses = await statusPageClient.CheckAllServicesAsync(cancellationToken);

				List<ServiceStatus> operational = serviceStatuses
					.Where(s => s.IsOperational)
					.OrderBy(s => s.ServiceName)
					.ToList();

				List<ServiceStatus> degraded = serviceStatuses
					.Where(s => !s.IsOperational)
					.OrderBy(s => s.ServiceName)
					.ToList();

				StringBuilder messageBuilder = new();
				messageBuilder.AppendLine("🌐 <b>Internet Service Status</b>");
				messageBuilder.AppendLine();

				if (degraded.Count > 0) {
					messageBuilder.AppendLine("🔴 <b>Services with Issues:</b>");
					foreach (ServiceStatus service in degraded) {
						messageBuilder.AppendLine($"  • <b>{service.ServiceName}</b>");
						if (!string.IsNullOrWhiteSpace(service.Description)) {
							messageBuilder.AppendLine($"    {service.Description}");
						}
					}
					messageBuilder.AppendLine();
				}

				if (operational.Count > 0) {
					messageBuilder.AppendLine($"✅ <b>Operational ({operational.Count} services):</b>");
					messageBuilder.AppendLine(string.Join(", ", operational.Select(s => s.ServiceName)));
				}

				messageBuilder.AppendLine();
				messageBuilder.AppendLine($"<i>Checked {serviceStatuses.Count} services</i>");

				await telegramBotClient.EditMessageText(
					chatId: command.Chat.Id,
					messageId: statusMessage.MessageId,
					text: messageBuilder.ToString(),
					parseMode: ParseMode.Html,
					cancellationToken: cancellationToken
				);
			} catch (Exception exc) {
				logger.LogError(exc, "Failed to check internet service status");
				await telegramBotClient.EditMessageText(
					chatId: command.Chat.Id,
					messageId: statusMessage.MessageId,
					text: "❌ Failed to check service status. Please try again later.",
					cancellationToken: cancellationToken
				);
			}
		}
	}
}
