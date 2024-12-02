using BotNet.Commands.Primbon;
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
		private static readonly RateLimiter RATE_LIMITER = RateLimiter.PerChat(2, TimeSpan.FromMinutes(2));
		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly PrimbonScraper _primbonScraper = primbonScraper;
		private readonly ChineseCalendarScraper _chineseCalendarScraper = chineseCalendarScraper;

		public async Task Handle(PrimbonCommand command, CancellationToken cancellationToken) {
			try {
				RATE_LIMITER.ValidateActionRate(command.Chat.Id, command.Sender.Id);

				(string javaneseDate, string sangar, string restriction) = await _primbonScraper.GetTaliwangkeAsync(
					date: command.Date,
					cancellationToken: cancellationToken
				);
				(string title, string[] traits) = await _primbonScraper.GetKamarokamAsync(
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
				) = await _chineseCalendarScraper.GetYellowCalendarAsync(
					date: command.Date,
					cancellationToken: cancellationToken
				);

				await _telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $$"""
						<b>{{javaneseDate}}</b>

						<b>Petung Hari Baik</b>
						{{title}}: {{string.Join(", ", traits)}}

						<b>Hari Larangan</b>
						{{sangar}}! {{restriction}}

						<b>Chinese Calendar</b>
						Clash: {{clash}} Evil: {{evil}}
						God of Joy: {{godOfJoy}}
						God of Happiness: {{godOfHappiness}}
						God of Wealth: {{godOfWealth}}
						Auspicious Activities: {{string.Join(", ", auspiciousActivities)}}
						Inauspicious Activities: {{string.Join(", ", inauspiciousActivities)}}
						""",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
					cancellationToken: cancellationToken
				);
			} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
				await _telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"Coba lagi {cooldown}.",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
					cancellationToken: cancellationToken
				);
			}
		}
	}
}
