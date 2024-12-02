﻿using BotNet.Commands.Humor;
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

		public Task Handle(HumorCommand command, CancellationToken cancellationToken) {
			try {
				RATE_LIMITER.ValidateActionRate(command.Chat.Id, command.Sender.Id);
			} catch (RateLimitExceededException exc) {
				return _telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"Bentar ya saya mikir dulu jokenya. Coba lagi {exc.Cooldown}.",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
					cancellationToken: cancellationToken
				);
			}

			// Fire and forget
			Task.Run(async () => {
				try {
					(string title, byte[] image) = await _programmerHumorScraper.GetRandomJokeAsync(cancellationToken);
					using MemoryStream imageStream = new(image);

					await _telegramBotClient.SendPhoto(
						chatId: command.Chat.Id,
						photo: new InputFileStream(imageStream, "joke.webp"),
						caption: title,
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
