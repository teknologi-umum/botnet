using BotNet.Commands;
using BotNet.Commands.AI.OpenAI;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.SenderAggregate;
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

		public async Task Handle(AskCommand askCommand, CancellationToken cancellationToken) {
			try {
				OpenAITextPromptHandler.CHAT_RATE_LIMITER.ValidateActionRate(
					chatId: askCommand.Command.Chat.Id,
					userId: askCommand.Command.Sender.Id
				);
			} catch (RateLimitExceededException exc) {
				await _telegramBotClient.SendTextMessageAsync(
					chatId: askCommand.Command.Chat.Id,
					text: $"<code>Anda terlalu banyak memanggil AI. Coba lagi {exc.Cooldown}.</code>",
					parseMode: ParseMode.Html,
					replyToMessageId: askCommand.Command.MessageId,
					cancellationToken: cancellationToken
				);
				return;
			}

			// Fire and forget
			Task _ = Task.Run(async () => {
				List<ChatMessage> messages = [
					ChatMessage.FromText("system", "The following is a conversation with an AI assistant. The assistant is helpful, creative, clever, and very friendly."),
					ChatMessage.FromText("user", askCommand.Prompt)
				];

				messages.AddRange(
					from message in askCommand.Thread.Take(10).Reverse()
					select ChatMessage.FromText(
						role: message.Sender.ChatGPTRole,
						text: message.Text
					)
				);

				Message responseMessage = await _telegramBotClient.SendTextMessageAsync(
					chatId: askCommand.Command.Chat.Id,
					text: MarkdownV2Sanitizer.Sanitize("… ⏳"),
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: askCommand.Command.MessageId
				);

				string response = await _openAIClient.ChatAsync(
					model: askCommand switch {
						({ Command: { Sender: VIPSender } or { Chat: HomeGroupChat } }) => "gpt-4-1106-preview",
						_ => "gpt-3.5-turbo"
					},
					messages: messages,
					maxTokens: 512,
					cancellationToken: cancellationToken
				);

				// Finalize message
				try {
					responseMessage = await telegramBotClient.EditMessageTextAsync(
						chatId: askCommand.Command.Chat.Id,
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
						replyToMessage: askCommand.Command,
						callSign: "AI"
					)
				);
			});
		}
	}
}
