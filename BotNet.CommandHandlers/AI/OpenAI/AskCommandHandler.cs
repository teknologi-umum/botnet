using BotNet.Commands;
using BotNet.Commands.AI.OpenAI;
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
	public sealed class AskCommandHandler(
		ITelegramBotClient telegramBotClient,
		OpenAIClient openAIClient,
		ITelegramMessageCache telegramMessageCache,
		ILogger<AskCommandHandler> logger
	) : ICommandHandler<AskCommand> {
		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly OpenAIClient _openAIClient = openAIClient;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;
		private readonly ILogger<AskCommandHandler> _logger = logger;

		public async Task Handle(AskCommand command, CancellationToken cancellationToken) {
			try {
				OpenAITextPromptHandler.CHAT_RATE_LIMITER.ValidateActionRate(
					chatId: command.ChatId,
					userId: command.SenderId
				);
			} catch (RateLimitExceededException exc) {
				await _telegramBotClient.SendTextMessageAsync(
					chatId: command.ChatId,
					text: $"<code>Anda terlalu banyak memanggil AI. Coba lagi {exc.Cooldown}.</code>",
					parseMode: ParseMode.Html,
					replyToMessageId: command.PromptMessageId,
					cancellationToken: cancellationToken
				);
				return;
			}

			// Fire and forget
			Task _ = Task.Run(async () => {
				List<ChatMessage> messages = [
					ChatMessage.FromText("system", "The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly."),
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
					message: AIResponseMessage.FromMessage(responseMessage, "AI")
				);
			});
		}
	}
}
