using BotNet.Commands;
using BotNet.Commands.AI.OpenAI;
using BotNet.Commands.AI.Stability;
using BotNet.Commands.Art;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.SenderAggregate;
using BotNet.Services.MarkdownV2;
using BotNet.Services.RateLimit;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Art {
	public sealed class ArtCommandHandler(
		ITelegramBotClient telegramBotClient,
		ICommandQueue commandQueue
	) : ICommandHandler<ArtCommand> {
		internal static readonly RateLimiter IMAGE_GENERATION_RATE_LIMITER = RateLimiter.PerUser(1, TimeSpan.FromMinutes(5));

		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly ICommandQueue _commandQueue = commandQueue;

		public Task Handle(ArtCommand command, CancellationToken cancellationToken) {
			try {
				IMAGE_GENERATION_RATE_LIMITER.ValidateActionRate(command.Chat.Id, command.Sender.Id);
			} catch (RateLimitExceededException exc) {
				return _telegramBotClient.SendTextMessageAsync(
					chatId: command.Chat.Id,
					text: $"Anda belum mendapat giliran. Coba lagi {exc.Cooldown}.",
					parseMode: ParseMode.Html,
					replyToMessageId: command.PromptMessageId,
					cancellationToken: cancellationToken
				);
			}

			// Fire and forget
			Task.Run(async () => {
				try {
					switch (command) {
						case { Sender: VIPSender }: {
								Message busyMessage = await _telegramBotClient.SendTextMessageAsync(
									chatId: command.Chat.Id,
									text: "Generating image… ⏳",
									parseMode: ParseMode.Markdown,
									replyToMessageId: command.PromptMessageId,
									cancellationToken: cancellationToken
								);

								await _commandQueue.DispatchAsync(
									new OpenAIImageGenerationPrompt(
										callSign: "GPT",
										prompt: command.Prompt,
										promptMessageId: command.PromptMessageId,
										responseMessageId: new(busyMessage.MessageId),
										chat: command.Chat,
										sender: command.Sender
									)
								);
							}
							break;
						case { Chat: HomeGroupChat }: {
								Message busyMessage = await _telegramBotClient.SendTextMessageAsync(
									chatId: command.Chat.Id,
									text: "Generating image… ⏳",
									parseMode: ParseMode.Markdown,
									replyToMessageId: command.PromptMessageId,
									cancellationToken: cancellationToken
								);

								await _commandQueue.DispatchAsync(
									new StabilityTextToImagePrompt(
										callSign: "GPT",
										prompt: command.Prompt,
										promptMessageId: command.PromptMessageId,
										responseMessageId: new(busyMessage.MessageId),
										chat: command.Chat,
										sender: command.Sender
									)
								);
							}
							break;
						default:
							await _telegramBotClient.SendTextMessageAsync(
								chatId: command.Chat.Id,
								text: MarkdownV2Sanitizer.Sanitize("Image generation tidak bisa dipakai di sini."),
								parseMode: ParseMode.MarkdownV2,
								replyToMessageId: command.PromptMessageId,
								cancellationToken: cancellationToken
							);
							break;
					}
				} catch (OperationCanceledException) {
					// Terminate gracefully
				}
			});

			return Task.CompletedTask;
		}
	}
}
