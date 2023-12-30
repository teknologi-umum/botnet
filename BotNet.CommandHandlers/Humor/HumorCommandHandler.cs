using BotNet.Commands.Humor;
using BotNet.Services.ProgrammerHumor;
using BotNet.Services.RateLimit;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Humor {
	public sealed class HumorCommandHandler(
		ITelegramBotClient telegramBotClient,
		ProgrammerHumorScraper programmerHumorScraper
	) : ICommandHandler<HumorCommand> {
		private static readonly RateLimiter RATE_LIMITER = RateLimiter.PerChat(2, TimeSpan.FromMinutes(2));

		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly ProgrammerHumorScraper _programmerHumorScraper = programmerHumorScraper;

		public async Task Handle(HumorCommand command, CancellationToken cancellationToken) {
			try {
				RATE_LIMITER.ValidateActionRate(command.ChatId, command.SenderId);

				(string title, byte[] image) = await _programmerHumorScraper.GetRandomJokeAsync(cancellationToken);
				using MemoryStream imageStream = new(image);

				await _telegramBotClient.SendPhotoAsync(
					chatId: command.ChatId,
					photo: new InputFileStream(imageStream, "joke.webp"),
					caption: title,
					cancellationToken: cancellationToken
				);
			} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
				await _telegramBotClient.SendTextMessageAsync(
					chatId: command.ChatId,
					text: $"Bentar ya saya mikir dulu jokenya. Coba lagi {cooldown}.",
					parseMode: ParseMode.Html,
					replyToMessageId: command.CommandMessageId,
					cancellationToken: cancellationToken
				);
			}
		}
	}
}
