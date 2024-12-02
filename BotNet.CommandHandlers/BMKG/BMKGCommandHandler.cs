using BotNet.Commands.BMKG;
using BotNet.Services.BMKG;
using BotNet.Services.RateLimit;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.BMKG {
	public sealed class BMKGCommandHandler(
		ITelegramBotClient telegramBotClient,
		LatestEarthQuake latestEarthQuake
	) : ICommandHandler<BMKGCommand> {
		private static readonly RateLimiter RATE_LIMITER = RateLimiter.PerChat(3, TimeSpan.FromMinutes(2));

		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly LatestEarthQuake _latestEarthQuake = latestEarthQuake;

		public Task Handle(BMKGCommand command, CancellationToken cancellationToken) {
			try {
				RATE_LIMITER.ValidateActionRate(command.Chat.Id, command.Sender.Id);
			} catch (RateLimitExceededException exc) {
				return _telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"Sabar dulu ya, tunggu giliran yang lain. Coba lagi {exc.Cooldown}.",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters {
						MessageId = command.CommandMessageId
					},
					cancellationToken: cancellationToken
				);
			}

			// Fire and forget
			Task.Run(async () => {
				try {
					(string text, string shakemapUrl) = await _latestEarthQuake.GetLatestAsync();

					await _telegramBotClient.SendPhoto(
						chatId: command.Chat.Id,
						photo: new InputFileUrl(shakemapUrl),
						caption: text,
						replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
						parseMode: ParseMode.Html,
						cancellationToken: cancellationToken
					);
				} catch (OperationCanceledException) {
					// Terminate gracefully
				}
			});

			return Task.CompletedTask;
		}
	}
}
