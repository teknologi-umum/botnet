using BotNet.Commands.BMKG;
using Mediator;
using BotNet.Services.BMKG;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.BMKG {
	public sealed class BmkgCommandHandler(
		ITelegramBotClient telegramBotClient,
		LatestEarthQuake latestEarthQuake,
		ILogger<BmkgCommandHandler> logger
	) : ICommandHandler<BmkgCommand> {
		private static readonly RateLimiter RateLimiter = RateLimiter.PerChat(3, TimeSpan.FromMinutes(2));

		public ValueTask<Unit> Handle(BmkgCommand command, CancellationToken cancellationToken) {
			try {
				RateLimiter.ValidateActionRate(command.Chat.Id, command.Sender.Id);
			} catch (RateLimitExceededException exc) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"Sabar dulu ya, tunggu giliran yang lain. Coba lagi {exc.Cooldown}.",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters {
						MessageId = command.CommandMessageId
					},
					cancellationToken: cancellationToken
				);
						return default;
			}

			// Fire and forget
			BackgroundTask.Run(async () => {
				(string text, string shakemapUrl) = await latestEarthQuake.GetLatestAsync();

				await telegramBotClient.SendPhoto(
					chatId: command.Chat.Id,
					photo: new InputFileUrl(shakemapUrl),
					caption: text,
					replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
					parseMode: ParseMode.Html,
					cancellationToken: cancellationToken
				);
			}, logger);

			return default;
		}
	}
}
