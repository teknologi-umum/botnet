using BotNet.Commands;
using BotNet.Commands.AI.Gemini;
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

namespace BotNet.CommandHandlers.AI.Gemini {
	public sealed class GeminiTextPromptHandler(
		ITelegramBotClient telegramBotClient,
		GeminiClient geminiClient,
		ITelegramMessageCache telegramMessageCache,
		CommandPriorityCategorizer commandPriorityCategorizer,
		ILogger<GeminiTextPromptHandler> logger
	) : ICommandHandler<GeminiTextPrompt> {
		internal static readonly RateLimiter CHAT_RATE_LIMITER = RateLimiter.PerChat(60, TimeSpan.FromMinutes(1));

		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly GeminiClient _geminiClient = geminiClient;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;
		private readonly CommandPriorityCategorizer _commandPriorityCategorizer = commandPriorityCategorizer;
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
				List<Content> messages = [];

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

				// Finalize message
				try {
					responseMessage = await telegramBotClient.EditMessageTextAsync(
						chatId: textPrompt.Command.Chat.Id,
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
