using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.ChineseCalendar;
using BotNet.Services.Primbon;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Primbon {
		private static readonly RateLimiter RATE_LIMITER = RateLimiter.PerChat(2, TimeSpan.FromMinutes(2));
		public static async Task GetKamarokamAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			DateOnly date;

			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument
				&& commandArgument.Length > 0) {
				if (!DateOnly.TryParseExact(commandArgument, "d-M-yyyy", out date)
					&& !DateOnly.TryParseExact(commandArgument, "yyyy-M-d", out date)
					&& !DateOnly.TryParseExact(commandArgument, "d/M/yyyy", out date)
					&& !DateOnly.TryParseExact(commandArgument, "yyyy/M/d", out date)
					&& !DateOnly.TryParseExact(commandArgument, "d MMM yyyy", out date)
					&& !DateOnly.TryParseExact(commandArgument, "d MMMM yyyy", out date)) {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: "<code>Format tanggal salah.</code>",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
					return;
				}
			} else {
				DateTime datetime = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7)).Date;
				date = new(datetime.Year, datetime.Month, datetime.Day);
			}

			try {
				RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);

				(string title, string[] traits) = await serviceProvider.GetRequiredService<PrimbonScraper>().GetKamarokamAsync(
					date: date,
					cancellationToken: cancellationToken
				);
				(
					string clash,
					string evil,
					string godOfJoy,
					string godOfHappiness,
					string godOfWealth,
					string[] auspiciousActivities,
					string[] inauspiciousActivities
				) = await serviceProvider.GetRequiredService<ChineseCalendarScraper>().GetYellowCalendarAsync(
					date: date,
					cancellationToken: cancellationToken
				);

				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: $$"""
						<b>{{date:dd MMMM yyyy}}</b>

						<b>Pentung Kamarokam</b>
						{{title}}: {{string.Join(", ", traits)}}

						<b>Chinese Calendar</b>
						Clash: {{clash}} Evil: {{evil}}
						God of Joy: {{godOfJoy}}
						God of Happiness: {{godOfHappiness}}
						God of Wealth: {{godOfWealth}}
						Auspicious Activities: {{string.Join(", ", auspiciousActivities)}}
						Inauspicious Activities: {{string.Join(", ", inauspiciousActivities)}}
						""",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: $"Coba lagi {cooldown}.",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			}
		}
	}
}
