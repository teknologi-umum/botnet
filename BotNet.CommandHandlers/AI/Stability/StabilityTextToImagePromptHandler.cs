using BotNet.Commands;
using BotNet.Commands.AI.Stability;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.CommandPrioritization;
using BotNet.Services.Stability.Models;
using BotNet.Services.Stability.Skills;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.AI.Stability {
	public sealed class StabilityTextToImagePromptHandler(
		ITelegramBotClient telegramBotClient,
		ImageGenerationBot imageGenerationBot,
		ITelegramMessageCache telegramMessageCache,
		CommandPriorityCategorizer commandPriorityCategorizer
	) : ICommandHandler<StabilityTextToImagePrompt> {
		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly ImageGenerationBot _imageGenerationBot = imageGenerationBot;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;
		private readonly CommandPriorityCategorizer _commandPriorityCategorizer = commandPriorityCategorizer;

		public Task Handle(StabilityTextToImagePrompt command, CancellationToken cancellationToken) {
			// Fire and forget
			Task.Run(async () => {
				try {
					byte[] generatedImage;
					try {
						generatedImage = await _imageGenerationBot.GenerateImageAsync(
							prompt: command.Prompt,
							cancellationToken: cancellationToken
						);
					} catch (ContentFilteredException exc) {
						await _telegramBotClient.EditMessageTextAsync(
							chatId: command.Chat.Id,
							messageId: command.ResponseMessageId,
							text: $"<code>{exc.Message ?? "Content filtered."}</code>",
							parseMode: ParseMode.Html,
							cancellationToken: cancellationToken
						);
						return;
					} catch {
						await _telegramBotClient.EditMessageTextAsync(
							chatId: command.Chat.Id,
							messageId: command.ResponseMessageId,
							text: "<code>Failed to generate image.</code>",
							parseMode: ParseMode.Html,
							cancellationToken: cancellationToken
						);
						return;
					}

					// Delete busy message
					try {
						await _telegramBotClient.DeleteMessageAsync(
							chatId: command.Chat.Id,
							messageId: command.ResponseMessageId,
							cancellationToken: cancellationToken
						);
					} catch (OperationCanceledException) {
						return;
					}

					// Send generated image
					using MemoryStream generatedImageStream = new(generatedImage);
					Message responseMessage = await _telegramBotClient.SendPhotoAsync(
						chatId: command.Chat.Id,
						photo: new InputFileStream(generatedImageStream, "art.png"),
						replyToMessageId: command.PromptMessageId,
						cancellationToken: cancellationToken
					);

					// Track thread
					_telegramMessageCache.Add(
						NormalMessage.FromMessage(responseMessage, _commandPriorityCategorizer)
					);
				} catch (OperationCanceledException) {
					// Terminate gracefully
					// TODO: tie up loose ends
				}
			});

			return Task.CompletedTask;
		}
	}
}
