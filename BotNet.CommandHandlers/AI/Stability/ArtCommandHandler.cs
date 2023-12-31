using BotNet.Commands;
using BotNet.Commands.AI.Stability;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Services.MarkdownV2;
using BotNet.Services.RateLimit;
using BotNet.Services.Stability.Models;
using BotNet.Services.Stability.Skills;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.AI.Stability {
	public sealed class ArtCommandHandler(
		ITelegramBotClient telegramBotClient,
		ImageGenerationBot imageGenerationBot,
		ITelegramMessageCache telegramMessageCache
	) : ICommandHandler<ArtCommand> {
		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly ImageGenerationBot _imageGenerationBot = imageGenerationBot;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;

		public Task Handle(ArtCommand command, CancellationToken cancellationToken) {
			try {
				StabilityTextToImagePromptHandler.IMAGE_GENERATION_RATE_LIMITER.ValidateActionRate(command.ChatId, command.SenderId);
			} catch (RateLimitExceededException exc) {
				return _telegramBotClient.SendTextMessageAsync(
					chatId: command.ChatId,
					text: $"Anda belum mendapat giliran. Coba lagi {exc.Cooldown}.",
					parseMode: ParseMode.Html,
					replyToMessageId: command.PromptMessageId,
					cancellationToken: cancellationToken
				);
			}

			// Fire and forget
			Task.Run(async () => {
				try {
					Message responseMessage = await _telegramBotClient.SendTextMessageAsync(
						chatId: command.ChatId,
						text: MarkdownV2Sanitizer.Sanitize("Generating image… ⏳"),
						parseMode: ParseMode.MarkdownV2,
						replyToMessageId: command.PromptMessageId
					);

					byte[] generatedImage;
					try {
						generatedImage = await _imageGenerationBot.GenerateImageAsync(
							prompt: command.Prompt,
							cancellationToken: cancellationToken
						);
					} catch (ContentFilteredException exc) {
						await _telegramBotClient.EditMessageTextAsync(
							chatId: command.ChatId,
							messageId: responseMessage.MessageId,
							text: $"<code>{exc.Message ?? "Content filtered."}</code>",
							parseMode: ParseMode.Html,
							cancellationToken: cancellationToken
						);
						return;
					} catch {
						await _telegramBotClient.EditMessageTextAsync(
							chatId: command.ChatId,
							messageId: responseMessage.MessageId,
							text: "<code>Failed to generate image.</code>",
							parseMode: ParseMode.Html,
							cancellationToken: cancellationToken
						);
						return;
					}

					// Delete busy message
					try {
						await _telegramBotClient.DeleteMessageAsync(
							chatId: command.ChatId,
							messageId: responseMessage.MessageId,
							cancellationToken: cancellationToken
						);
					} catch (OperationCanceledException) {
						return;
					}

					// Send generated image
					using MemoryStream generatedImageStream = new(generatedImage);
					responseMessage = await _telegramBotClient.SendPhotoAsync(
						chatId: command.ChatId,
						photo: new InputFileStream(generatedImageStream, "art.png"),
						replyToMessageId: command.PromptMessageId,
						cancellationToken: cancellationToken
					);

					// Track thread
					_telegramMessageCache.Add(
						NormalMessage.FromMessage(responseMessage)
					);
				} catch (OperationCanceledException) {
					// Terminate gracefully
				}
			});

			return Task.CompletedTask;
		}
	}
}
