﻿using BotNet.CommandHandlers.AI.RateLimit;
using BotNet.Commands;
using BotNet.Commands.AI.Gemini;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.CommandPrioritization;
using BotNet.Commands.SenderAggregate;
using BotNet.Services.Gemini;
using BotNet.Services.Gemini.Models;
using BotNet.Services.RateLimit;
using BotNet.Services.TelegramClient;
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
		internal static readonly RateLimiter CHAT_RATE_LIMITER = RateLimiter.PerUserPerChat(5, TimeSpan.FromMinutes(5));
		internal static readonly RateLimiter VIP_CHAT_RATE_LIMITER = RateLimiter.PerUserPerChat(5, TimeSpan.FromMinutes(2));

		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly GeminiClient _geminiClient = geminiClient;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;
		private readonly CommandPriorityCategorizer _commandPriorityCategorizer = commandPriorityCategorizer;
		private readonly ICommandQueue _commandQueue = commandQueue;
		private readonly ILogger<GeminiTextPromptHandler> _logger = logger;

		public Task Handle(GeminiTextPrompt textPrompt, CancellationToken cancellationToken) {
			if (textPrompt.Command.Chat is GroupChat) {
				try {
					AIRateLimiters.GROUP_CHAT_RATE_LIMITER.ValidateActionRate(
						chatId: textPrompt.Command.Chat.Id,
						userId: textPrompt.Command.Sender.Id
					);
				} catch (RateLimitExceededException exc) {
					return _telegramBotClient.SendTextMessageAsync(
						chatId: textPrompt.Command.Chat.Id,
						text: $"<code>Anda terlalu banyak memanggil AI. Coba lagi {exc.Cooldown} atau lanjutkan di private chat.</code>",
						parseMode: ParseMode.Html,
						replyToMessageId: textPrompt.Command.MessageId,
						replyMarkup: new InlineKeyboardMarkup(
							InlineKeyboardButton.WithUrl("Private chat 💬", "t.me/TeknumBot")
						),
						cancellationToken: cancellationToken
					);
				}
			} else {
				try {
					if (textPrompt.Command.Sender is VIPSender) {
						VIP_CHAT_RATE_LIMITER.ValidateActionRate(
							chatId: textPrompt.Command.Chat.Id,
							userId: textPrompt.Command.Sender.Id
						);
					} else {
						CHAT_RATE_LIMITER.ValidateActionRate(
							chatId: textPrompt.Command.Chat.Id,
							userId: textPrompt.Command.Sender.Id
						);
					}
				} catch (RateLimitExceededException exc) {
					return _telegramBotClient.SendTextMessageAsync(
						chatId: textPrompt.Command.Chat.Id,
						text: $"<code>Anda terlalu banyak memanggil AI. Coba lagi {exc.Cooldown}.</code>",
						parseMode: ParseMode.Html,
						replyToMessageId: textPrompt.Command.MessageId,
						cancellationToken: cancellationToken
					);
				}
			}

			// Fire and forget
			Task.Run(async () => {
				List<Content> messages = [
					Content.FromText("user", "Act as an AI assistant. The assistant is helpful, creative, direct, concise, and always get to the point."),
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

				// Merge user message with replied to message if thread is initiated by replying to another user
				if (messages.Count > 0
					&& messages[^1].Role == "user") {
					messages[^1].Add(Content.FromText("user", textPrompt.Prompt));
				} else {
					messages.Add(Content.FromText("user", textPrompt.Prompt));
				}

				string response = await _geminiClient.ChatAsync(
					messages: messages,
					maxTokens: 512,
					cancellationToken: cancellationToken
				);

				// Send response
				Message responseMessage;
				try {
					responseMessage = await telegramBotClient.SendTextMessageAsync(
						chatId: textPrompt.Command.Chat.Id,
						text: response,
						parseModes: [ParseMode.MarkdownV2, ParseMode.Markdown, ParseMode.Html],
						replyToMessageId: textPrompt.Command.MessageId,
						replyMarkup: new InlineKeyboardMarkup(
							InlineKeyboardButton.WithUrl(
								text: "Generated by Google Gemini 1.5 Flash",
								url: "https://deepmind.google/technologies/gemini/"
							)
						),
						cancellationToken: cancellationToken
					);
				} catch (Exception exc) {
					_logger.LogError(exc, null);
					await telegramBotClient.SendTextMessageAsync(
						chatId: textPrompt.Command.Chat.Id,
						text: "😵",
						parseMode: ParseMode.Html,
						replyToMessageId: textPrompt.Command.MessageId,
						cancellationToken: cancellationToken
					);
					return;
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
