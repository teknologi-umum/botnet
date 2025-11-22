using BotNet.Commands.Pick;
using Mediator;
using BotNet.Services.MarkdownV2;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Pick {
	public sealed class PickCommandHandler(
		ITelegramBotClient telegramBotClient,
		ILogger<PickCommandHandler> logger
	) : ICommandHandler<PickCommand> {
		private static readonly RateLimiter RateLimiter = RateLimiter.PerUserPerChat(5, TimeSpan.FromMinutes(1));

		public async ValueTask<Unit> Handle(PickCommand command, CancellationToken cancellationToken) {
			// Rate limiting
			try {
				RateLimiter.ValidateActionRate(command.Chat.Id, command.Sender.Id);
			} catch (RateLimitExceededException exc) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"Rate limit exceeded. Try again {exc.Cooldown}.",
					cancellationToken: cancellationToken
				);
				return default;
		return default;
			}

			try {
				// Randomly pick one option
				string picked = command.Options[Random.Shared.Next(command.Options.Length)];

				// Format response
				string response = $"🎲 I pick: *{MarkdownV2Sanitizer.Sanitize(picked)}*";

				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: response,
					parseMode: ParseMode.MarkdownV2,
					replyParameters: new Telegram.Bot.Types.ReplyParameters {
						MessageId = command.MessageId
					},
					cancellationToken: cancellationToken
				);
			} catch (Exception exc) {
				logger.LogError(exc, "Failed to process pick command");
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: "Failed to pick an option. Please try again.",
					cancellationToken: cancellationToken
				);
			}
	return default;
		}
	}
}
