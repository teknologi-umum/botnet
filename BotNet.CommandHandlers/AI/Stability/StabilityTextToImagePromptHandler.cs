using BotNet.Commands;
using BotNet.Commands.AI.Stability;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Services.RateLimit;
using BotNet.Services.Stability.Models;
using BotNet.Services.Stability.Skills;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.AI.Stability {
	public sealed class StabilityTextToImagePromptHandler(
		ITelegramBotClient telegramBotClient,
		BotNet.Services.Stability.Skills.ImageGenerationBot imageGenerationBot,
		ITelegramMessageCache telegramMessageCache
	) : ICommandHandler<StabilityTextToImagePrompt> {
		private static readonly RateLimiter IMAGE_GENERATION_RATE_LIMITER = RateLimiter.PerUser(1, TimeSpan.FromMinutes(5));

		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly ImageGenerationBot _imageGenerationBot = imageGenerationBot;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;

		public Task Handle(StabilityTextToImagePrompt command, CancellationToken cancellationToken) {
			// Fire and forget
			Task.Run(async () => {
				byte[] generatedImage;
				try {
					generatedImage = await _imageGenerationBot.GenerateImageAsync(
						prompt: command.Prompt,
						cancellationToken: CancellationToken.None
					);
				} catch (ContentFilteredException exc) {
					await _telegramBotClient.EditMessageTextAsync(
						chatId: command.ChatId,
						messageId: command.ResponseMessageId,
						text: $"<code>{exc.Message ?? "Content filtered."}</code>",
						parseMode: ParseMode.Html
					);
					return;
				} catch {
					await _telegramBotClient.EditMessageTextAsync(
						chatId: command.ChatId,
						messageId: command.ResponseMessageId,
						text: "<code>Failed to generate image.</code>",
						parseMode: ParseMode.Html
					);
					return;
				}

				// Delete busy message
				try {
					await _telegramBotClient.DeleteMessageAsync(
						chatId: command.ChatId,
						messageId: command.ResponseMessageId,
						cancellationToken: cancellationToken
					);
				} catch (OperationCanceledException) {
					return;
				}

				// Send generated image
				using MemoryStream generatedImageStream = new(generatedImage);
				Message responseMessage = await _telegramBotClient.SendPhotoAsync(
					chatId: command.ChatId,
					photo: new InputFileStream(generatedImageStream, "art.png"),
					replyToMessageId: command.PromptMessageId,
					cancellationToken: cancellationToken
				);

				// Track thread
				_telegramMessageCache.Add(
					NormalMessage.FromMessage(responseMessage)
				);
			});

			return Task.CompletedTask;
		}
	}
}
