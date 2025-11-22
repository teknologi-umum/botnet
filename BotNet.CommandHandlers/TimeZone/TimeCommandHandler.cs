using System;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.TimeZone;
using BotNet.Services.RateLimit;
using BotNet.Services.TimeZone;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.TimeZone {
	public sealed class TimeCommandHandler(
		ITelegramBotClient telegramBotClient,
		TimeZoneService timeZoneService
	) : ICommandHandler<TimeCommand> {
		private static readonly RateLimiter RateLimiter = RateLimiter.PerUserPerChat(3, TimeSpan.FromMinutes(2));

		public async ValueTask<Unit> Handle(TimeCommand command, CancellationToken cancellationToken) {
			try {
				RateLimiter.ValidateActionRate(command.Command.Chat.Id, command.Command.Sender.Id);
			} catch (RateLimitExceededException exc) {
				await telegramBotClient.SendMessage(
					chatId: command.Command.Chat.Id,
					text: $"<code>Coba lagi {exc.Cooldown}</code>",
					parseMode: ParseMode.Html,
					replyParameters: new() {
						MessageId = command.Command.MessageId
					},
					cancellationToken: cancellationToken
				);
				return default;
			}

			TimeInfo timeInfo = timeZoneService.GetTimeInfo(command.CityOrTimeZone);

			if (!timeInfo.Success) {
				await telegramBotClient.SendMessage(
					chatId: command.Command.Chat.Id,
					text: $"❌ {timeInfo.ErrorMessage}",
					parseMode: ParseMode.Html,
					replyParameters: new() {
						MessageId = command.Command.MessageId
					},
					cancellationToken: cancellationToken
				);
				return default;
			}

			string utcOffsetString = timeInfo.UtcOffset >= TimeSpan.Zero
				? $"UTC+{timeInfo.UtcOffset.Hours}"
				: $"UTC{timeInfo.UtcOffset.Hours}";

			string message = $"""
				🕒 <b>{command.CityOrTimeZone}</b>
				🌍 {timeInfo.TimeZoneId}
				⏰ {timeInfo.LocalTime:HH:mm:ss}
				📅 {timeInfo.LocalTime:dddd, MMMM dd, yyyy}
				🌐 {utcOffsetString}
				""";

			await telegramBotClient.SendMessage(
				chatId: command.Command.Chat.Id,
				text: message,
				parseMode: ParseMode.Html,
				replyParameters: new() {
					MessageId = command.Command.MessageId
				},
				cancellationToken: cancellationToken
			);
	return default;
		}
	}
}
