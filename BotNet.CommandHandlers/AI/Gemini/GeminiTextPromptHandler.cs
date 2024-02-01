using BotNet.CommandHandlers.Art;
using BotNet.Commands;
using BotNet.Commands.AI.Gemini;
using BotNet.Commands.AI.OpenAI;
using BotNet.Commands.AI.Stability;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.CommandPrioritization;
using BotNet.Commands.SenderAggregate;
using BotNet.Services.Gemini;
using BotNet.Services.Gemini.Models;
using BotNet.Services.MarkdownV2;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotNet.CommandHandlers.AI.Gemini {
	public sealed class GeminiTextPromptHandler(
		ITelegramBotClient telegramBotClient,
		GeminiClient geminiClient,
		ITelegramMessageCache telegramMessageCache,
		CommandPriorityCategorizer commandPriorityCategorizer,
		ICommandQueue commandQueue,
		ILogger<GeminiTextPromptHandler> logger
	) : ICommandHandler<GeminiTextPrompt> {
		internal static readonly RateLimiter CHAT_RATE_LIMITER = RateLimiter.PerChat(60, TimeSpan.FromMinutes(1));

		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly GeminiClient _geminiClient = geminiClient;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;
		private readonly CommandPriorityCategorizer _commandPriorityCategorizer = commandPriorityCategorizer;
		private readonly ICommandQueue _commandQueue = commandQueue;
		private readonly ILogger<GeminiTextPromptHandler> _logger = logger;

		public Task Handle(GeminiTextPrompt textPrompt, CancellationToken cancellationToken) {
			if (textPrompt.Command.Chat is not HomeGroupChat
				&& textPrompt.Command.Sender is not VIPSender) {
				return _telegramBotClient.SendTextMessageAsync(
					chatId: textPrompt.Command.Chat.Id,
					text: MarkdownV2Sanitizer.Sanitize("Gemini tidak bisa dipakai di sini."),
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: textPrompt.Command.MessageId,
					cancellationToken: cancellationToken
				);
			}

			try {
				CHAT_RATE_LIMITER.ValidateActionRate(
					chatId: textPrompt.Command.Chat.Id,
					userId: textPrompt.Command.Sender.Id
				);
			} catch (RateLimitExceededException exc) {
				return _telegramBotClient.SendTextMessageAsync(
					chatId: textPrompt.Command.Chat.Id,
					text: $"<code>Anda terlalu banyak memanggil AI. Coba lagi {exc.Cooldown}.</code>",
					parseMode: ParseMode.Html,
					replyToMessageId: textPrompt.Command.MessageId,
					cancellationToken: cancellationToken
				);
			}

			// Fire and forget
			Task.Run(async () => {
				List<Content> messages = [
					Content.FromText("user", "Act as an AI assistant. The assistant is helpful, creative, direct, concise, and always get to the point. When user asks for an image to be generated, the AI assistant should respond with \"ImageGeneration:\" followed by comma separated list of features to be expected from the generated image."),
					Content.FromText("model", "Sure.")
				];

				// Merge adjacent messages from same role
				foreach (MessageBase message in textPrompt.Thread.Reverse()) {
					Content content = Content.FromText(
						role: message.Sender.GeminiRole,
						text: message.Text
					);

					if (messages.Count > 0
						&& messages[^1].Role == message.Sender.GeminiRole) {
						messages[^1].Add(content);
					} else {
						messages.Add(content);
					}
				}

				// Trim thread longer than 10 messages
				while (messages.Count > 10) {
					messages.RemoveAt(0);
				}

				// Thread must start with user message
				while (messages.Count > 0
					&& messages[0].Role != "user") {
					messages.RemoveAt(0);
				}

				messages.Add(
					Content.FromText("user", textPrompt.Prompt)
				);

				Message responseMessage = await _telegramBotClient.SendTextMessageAsync(
					chatId: textPrompt.Command.Chat.Id,
					text: MarkdownV2Sanitizer.Sanitize("… ⏳"),
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: textPrompt.Command.MessageId
				);

				string response = await _geminiClient.ChatAsync(
					messages: messages,
					maxTokens: 512,
					cancellationToken: cancellationToken
				);

				// Handle image generation intent
				if (response.StartsWith("ImageGeneration:")) {
					if (textPrompt.Command.Sender is not VIPSender) {
						try {
							ArtCommandHandler.IMAGE_GENERATION_RATE_LIMITER.ValidateActionRate(textPrompt.Command.Chat.Id, textPrompt.Command.Sender.Id);
						} catch (RateLimitExceededException exc) {
							await _telegramBotClient.SendTextMessageAsync(
								chatId: textPrompt.Command.Chat.Id,
								text: $"Anda belum mendapat giliran. Coba lagi {exc.Cooldown}.",
								parseMode: ParseMode.Html,
								replyToMessageId: textPrompt.Command.MessageId,
								cancellationToken: cancellationToken
							);
							return;
						}
					}

					string imageGenerationPrompt = response.Substring(response.IndexOf(':') + 1).Trim();
					switch (textPrompt.Command) {
						case { Sender: VIPSender }:
							await _commandQueue.DispatchAsync(
								command: new OpenAIImageGenerationPrompt(
									callSign: "Gemini",
									prompt: imageGenerationPrompt,
									promptMessageId: textPrompt.Command.MessageId,
									responseMessageId: new(responseMessage.MessageId),
									chat: textPrompt.Command.Chat,
									sender: textPrompt.Command.Sender
								)
							);
							break;
						case { Chat: HomeGroupChat }:
							await _commandQueue.DispatchAsync(
								command: new StabilityTextToImagePrompt(
									callSign: "Gemini",
									prompt: imageGenerationPrompt,
									promptMessageId: textPrompt.Command.MessageId,
									responseMessageId: new(responseMessage.MessageId),
									chat: textPrompt.Command.Chat,
									sender: textPrompt.Command.Sender
								)
							);
							break;
						default:
							await _telegramBotClient.EditMessageTextAsync(
								chatId: textPrompt.Command.Chat.Id,
								messageId: responseMessage.MessageId,
								text: MarkdownV2Sanitizer.Sanitize("Image generation tidak bisa dipakai di sini."),
								parseMode: ParseMode.MarkdownV2,
								cancellationToken: cancellationToken
							);
							break;
					}
					return;
				}

				// Finalize message
				try {
					responseMessage = await telegramBotClient.EditMessageTextAsync(
						chatId: textPrompt.Command.Chat.Id,
						messageId: responseMessage.MessageId,
						text: MarkdownV2Sanitizer.Sanitize(response),
						parseMode: ParseMode.MarkdownV2,
						replyMarkup: new InlineKeyboardMarkup(
							InlineKeyboardButton.WithUrl(
								text: "Generated by Google Gemini Pro",
								url: "https://deepmind.google/technologies/gemini/"
							)
						),
						cancellationToken: cancellationToken
					);
				} catch (Exception exc) {
					_logger.LogError(exc, null);
					throw;
				}

				// Track thread
				_telegramMessageCache.Add(
					message: AIResponseMessage.FromMessage(
						message: responseMessage,
						replyToMessage: textPrompt.Command,
						callSign: "Gemini",
						commandPriorityCategorizer: _commandPriorityCategorizer
					)
				);
			});

			return Task.CompletedTask;
		}
	}
}
