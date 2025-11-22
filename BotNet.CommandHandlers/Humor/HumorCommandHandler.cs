using BotNet.Commands.Humor;
using BotNet.Services.ProgrammerHumor;
using Mediator;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Humor {
	public sealed class HumorCommandHandler(
		ITelegramBotClient telegramBotClient,
		ProgrammerHumorScraper programmerHumorScraper,
		ILogger<HumorCommandHandler> logger
	) : ICommandHandler<HumorCommand> {
		private static readonly RateLimiter RateLimiter = RateLimiter.PerChat(2, TimeSpan.FromMinutes(2));

		public async ValueTask<Unit> Handle(HumorCommand command, CancellationToken cancellationToken) {
			try {
				RateLimiter.ValidateActionRate(command.Chat.Id, command.Sender.Id);
			} catch (RateLimitExceededException exc) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"Bentar ya saya mikir dulu jokenya. Coba lagi {exc.Cooldown}.",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
					cancellationToken: cancellationToken
				);
				return default;
			}

			// Fire and forget
			BackgroundTask.Run(async () => {
				(string title, byte[] image) = await programmerHumorScraper.GetRandomJokeAsync(cancellationToken);
				using MemoryStream imageStream = new(image);

				await telegramBotClient.SendPhoto(
					chatId: command.Chat.Id,
					photo: new InputFileStream(imageStream, "joke.webp"),
					caption: title,
					cancellationToken: cancellationToken
				);
			}, logger);

			return default;
		}
	}
}
