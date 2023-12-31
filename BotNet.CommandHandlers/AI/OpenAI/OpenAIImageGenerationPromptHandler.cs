﻿using BotNet.Commands;
using BotNet.Commands.AI.OpenAI;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Services.OpenAI.Skills;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.AI.OpenAI {
	public sealed class OpenAIImageGenerationPromptHandler(
		ITelegramBotClient telegramBotClient,
		ImageGenerationBot imageGenerationBot,
		ITelegramMessageCache telegramMessageCache,
		ILogger<OpenAIImageGenerationPromptHandler> logger
	) : ICommandHandler<OpenAIImageGenerationPrompt> {
		private static readonly RateLimiter IMAGE_GENERATION_RATE_LIMITER = RateLimiter.PerUser(1, TimeSpan.FromMinutes(5));

		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly ImageGenerationBot _imageGenerationBot = imageGenerationBot;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;
		private readonly ILogger<OpenAIImageGenerationPromptHandler> _logger = logger;

		public Task Handle(OpenAIImageGenerationPrompt command, CancellationToken cancellationToken) {
			try {
				IMAGE_GENERATION_RATE_LIMITER.ValidateActionRate(command.ChatId, command.SenderId);
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
					Uri generatedImageUrl;
					try {
						generatedImageUrl = await _imageGenerationBot.GenerateImageAsync(
							prompt: command.Prompt,
							cancellationToken: cancellationToken
						);
					} catch (Exception exc) {
						_logger.LogError(exc, "Could not generate image");
						await _telegramBotClient.EditMessageTextAsync(
							chatId: command.ChatId,
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
							chatId: command.ChatId,
							messageId: command.ResponseMessageId,
							cancellationToken: cancellationToken
						);
					} catch (OperationCanceledException) {
						return;
					}

					// Send generated image
					Message responseMessage = await _telegramBotClient.SendPhotoAsync(
						chatId: command.ChatId,
						photo: new InputFileUrl(generatedImageUrl),
						replyToMessageId: command.PromptMessageId,
						cancellationToken: cancellationToken
					);

					// Track thread
					_telegramMessageCache.Add(
						NormalMessage.FromMessage(responseMessage)
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