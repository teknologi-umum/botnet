using BotNet.Commands;
using BotNet.Commands.AI.OpenAI;
using BotNet.Commands.AI.Stability;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.CommandPrioritization;
using BotNet.Services.MarkdownV2;
using BotNet.Services.OpenAI;
using BotNet.Services.OpenAI.Models;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.AI.OpenAI {
	public sealed class OpenAITextPromptHandler(
		ITelegramBotClient telegramBotClient,
		ICommandQueue commandQueue,
		ITelegramMessageCache telegramMessageCache,
		OpenAIClient openAIClient,
		ILogger<OpenAITextPromptHandler> logger
	) : ICommandHandler<OpenAITextPrompt> {
		internal static readonly RateLimiter CHAT_RATE_LIMITER = RateLimiter.PerUserPerChat(5, TimeSpan.FromMinutes(15));

		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly ICommandQueue _commandQueue = commandQueue;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;
		private readonly OpenAIClient _openAIClient = openAIClient;
		private readonly ILogger<OpenAITextPromptHandler> _logger = logger;

		public Task Handle(OpenAITextPrompt command, CancellationToken cancellationToken) {
			try {
				CHAT_RATE_LIMITER.ValidateActionRate(
					chatId: command.ChatId,
					userId: command.SenderId
				);
			} catch (RateLimitExceededException exc) {
				return _telegramBotClient.SendTextMessageAsync(
					chatId: command.ChatId,
					text: $"<code>Anda terlalu banyak memanggil AI. Coba lagi {exc.Cooldown}.</code>",
					parseMode: ParseMode.Html,
					replyToMessageId: command.PromptMessageId,
					cancellationToken: cancellationToken
				);
			}

			// Fire and forget
			Task.Run(async () => {
				List<ChatMessage> messages = [
					ChatMessage.FromText("system", "The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly. When user asks for an image to be generated, the AI assistant should respond with \"ImageGeneration:\" followed by comma separated list of features to be expected from the generated image."),
					ChatMessage.FromText("user", command.Prompt)
				];

				messages.AddRange(
					from message in command.Thread.Take(10).Reverse()
					select ChatMessage.FromText(
						role: message.SenderName switch {
							"AI" or "Bot" or "GPT" => "assistant",
							_ => "user"
						},
						text: message.Text
					)
				);

				Message responseMessage = await _telegramBotClient.SendTextMessageAsync(
					chatId: command.ChatId,
					text: MarkdownV2Sanitizer.Sanitize("… ⏳"),
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: command.PromptMessageId
				);

				string response = await _openAIClient.ChatAsync(
					model: command.CommandPriority switch {
						CommandPriority.VIPChat or CommandPriority.HomeGroupChat => "gpt-4-1106-preview",
						_ => "gpt-3.5-turbo"
					},
					messages: messages,
					maxTokens: 512,
					cancellationToken: cancellationToken
				);

				// Handle image generation intent
				if (response.StartsWith("ImageGeneration:")) {
					string imageGenerationPrompt = response.Substring(response.IndexOf(':') + 1).Trim();
					switch (command.CommandPriority) {
						case CommandPriority.VIPChat:
							await _commandQueue.DispatchAsync(
								command: new OpenAIImageGenerationPrompt(
									callSign: command.CallSign,
									prompt: imageGenerationPrompt,
									promptMessageId: command.PromptMessageId,
									responseMessageId: responseMessage.MessageId,
									chatId: command.ChatId,
									senderId: command.SenderId,
									commandPriority: command.CommandPriority
								)
							);
							break;
						case CommandPriority.HomeGroupChat:
							await _commandQueue.DispatchAsync(
								command: new StabilityTextToImagePrompt(
									callSign: command.CallSign,
									prompt: imageGenerationPrompt,
									promptMessageId: command.PromptMessageId,
									responseMessageId: responseMessage.MessageId,
									chatId: command.ChatId,
									senderId: command.SenderId,
									commandPriority: command.CommandPriority
								)
							);
							break;
						default:
							await _telegramBotClient.EditMessageTextAsync(
								chatId: command.ChatId,
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
						chatId: command.ChatId,
						messageId: responseMessage.MessageId,
						text: MarkdownV2Sanitizer.Sanitize(response),
						parseMode: ParseMode.MarkdownV2,
						cancellationToken: cancellationToken
					);
				} catch (Exception exc) {
					_logger.LogError(exc, null);
					throw;
				}

				// Track thread
				_telegramMessageCache.Add(
					message: AIResponseMessage.FromMessage(
						responseMessage,
						command.CallSign
					)
				);
			});

			return Task.CompletedTask;
		}
	}
}
