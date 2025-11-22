using System;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Commands.Soundtrack;
using BotNet.Services.RateLimit;
using BotNet.Services.Soundtrack;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Soundtrack {
	public sealed class SoundtrackCommandHandler(
		ITelegramBotClient telegramBotClient,
		SoundtrackProvider soundtrackProvider
	) : ICommandHandler<SoundtrackCommand> {
		private static readonly RateLimiter RateLimiter = RateLimiter.PerUserPerChat(1, TimeSpan.FromMinutes(10));

		public async ValueTask<Unit> Handle(SoundtrackCommand command, CancellationToken cancellationToken) {
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

			(SoundtrackSite first, SoundtrackSite second) = soundtrackProvider.GetRandomPicks();

			string message = $"""
				🎵 <b>Your coding soundtracks:</b>

				1️⃣ <b>{first.Name}</b>
				{first.Url}

				2️⃣ <b>{second.Name}</b>
				{second.Url}

				💡 <i>Get new picks every 10 minutes!</i>
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
