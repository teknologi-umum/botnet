using BotNet.Commands.Primbon;
using Mediator;
using BotNet.Services.ChineseCalendar;
using BotNet.Services.Primbon;
using BotNet.Services.RateLimit;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Primbon {
	public sealed class PrimbonCommandHandler(
		ITelegramBotClient telegramBotClient,
		PrimbonScraper primbonScraper,
		ChineseCalendarScraper chineseCalendarScraper
	) : ICommandHandler<PrimbonCommand> {
		private static readonly RateLimiter RateLimiter = RateLimiter.PerChat(2, TimeSpan.FromMinutes(2));

		public async ValueTask<Unit> Handle(PrimbonCommand command, CancellationToken cancellationToken) {
			try {
				RateLimiter.ValidateActionRate(command.Chat.Id, command.Sender.Id);

				(string javaneseDate, string sangar, string restriction) = await primbonScraper.GetTaliwangkeAsync(
					date: command.Date,
					cancellationToken: cancellationToken
				);
				(string title, string[] traits) = await primbonScraper.GetKamarokamAsync(
					date: command.Date,
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
				) = await chineseCalendarScraper.GetYellowCalendarAsync(
					date: command.Date,
					cancellationToken: cancellationToken
				);

				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"""
						<b>{javaneseDate}</b>

						<b>Petung Hari Baik</b>
						{title}: {string.Join(", ", traits)}

						<b>Hari Larangan</b>
						{sangar}! {restriction}

						<b>Chinese Calendar</b>
						Clash: {clash} Evil: {evil}
						God of Joy: {godOfJoy}
						God of Happiness: {godOfHappiness}
						God of Wealth: {godOfWealth}
						Auspicious Activities: {string.Join(", ", auspiciousActivities)}
						Inauspicious Activities: {string.Join(", ", inauspiciousActivities)}
						""",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
					cancellationToken: cancellationToken
				);
			} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"Coba lagi {cooldown}.",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
					cancellationToken: cancellationToken
				);
		return default;
			}
	return default;
		}
	}
}
