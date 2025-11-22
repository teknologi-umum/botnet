using BotNet.Commands.Privacy;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Privacy {
	public sealed class PrivacyCommandHandler(
		ITelegramBotClient telegramBotClient,
		ILogger<PrivacyCommandHandler> logger
	) : ICommandHandler<PrivacyCommand> {
		private static readonly RateLimiter RateLimiter = RateLimiter.PerUserPerChat(1, TimeSpan.FromMinutes(5));

		public Task Handle(PrivacyCommand command, CancellationToken cancellationToken) {
			try {
				RateLimiter.ValidateActionRate(command.Chat.Id, command.Sender.Id);
			} catch (RateLimitExceededException) {
				// Silently reject commands after rate limit exceeded
				return Task.CompletedTask;
			}

			// Fire and forget
			BackgroundTask.Run(async () => {
				try {
					string privacyMessage = """
						🔒 <b>Privacy Notice</b>

						This bot uses third-party services that may receive your data. When you use certain commands, your input is sent to external APIs:

						<b>AI Services (receive your messages):</b>
						• OpenAI - /ask, AI chat
						• Google Gemini - AI features
						• Stability AI - /art, image generation
						• Craiyon - image generation

						<b>Other Services:</b>
						• Google Maps - /map (location searches)
						• Piston - code execution (/python, /java, etc.)
						• OMDb - /movie (movie searches)
						• wttr.in - /weather (location queries)

						<b>⚠️ Important:</b>
						• Don't share passwords, API keys, or sensitive data
						• Your AI conversations are sent to third parties
						• Code you execute is processed on external servers

						<b>Full Details:</b>
						📄 Read the complete privacy notice at:
						https://github.com/teknologi-umum/botnet/blob/master/PRIVACY_NOTICE.md

						<i>This bot is open source. We don't store your personal data, but third-party services have their own privacy policies.</i>
						""";

					await telegramBotClient.SendMessage(
						chatId: command.Chat.Id,
						text: privacyMessage,
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
						cancellationToken: cancellationToken
					);
				} catch (OperationCanceledException) {
					// Terminate gracefully
				}
			}, logger);

			return Task.CompletedTask;
		}
	}
}
