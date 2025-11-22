using BotNet.Commands;
using Mediator;
using BotNet.Commands.AI.OpenAI;
using BotNet.Commands.AI.Stability;
using BotNet.Commands.Art;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.SenderAggregate;
using BotNet.Services.MarkdownV2;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Art {
	public sealed class ArtCommandHandler(
		ITelegramBotClient telegramBotClient,
		ICommandQueue commandQueue,
		ILogger<ArtCommandHandler> logger
	) : ICommandHandler<ArtCommand> {
		internal static readonly RateLimiter ImageGenerationRateLimiter = RateLimiter.PerUser(2, TimeSpan.FromMinutes(3));

		public async ValueTask<Unit> Handle(ArtCommand command, CancellationToken cancellationToken) {
			try {
				ImageGenerationRateLimiter.ValidateActionRate(command.Chat.Id, command.Sender.Id);
			} catch (RateLimitExceededException exc) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"Anda belum mendapat giliran. Coba lagi {exc.Cooldown}.",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters {
						MessageId = command.PromptMessageId
					},
					cancellationToken: cancellationToken
				);
						return default;
			}

			// Fire and forget
			BackgroundTask.Run(async () => {
				switch (command) {
					case { Sender: VipSender }: {
							Message busyMessage = await telegramBotClient.SendMessage(
								chatId: command.Chat.Id,
								text: "Generating image… ⏳",
								parseMode: ParseMode.Markdown,
								replyParameters: new ReplyParameters {
									MessageId = command.PromptMessageId
								},
								cancellationToken: cancellationToken
							);

							await commandQueue.DispatchAsync(
								new OpenAiImageGenerationPrompt(
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
							Message busyMessage = await telegramBotClient.SendMessage(
								chatId: command.Chat.Id,
								text: "Generating image… ⏳",
								parseMode: ParseMode.Markdown,
								replyParameters: new ReplyParameters {
									MessageId = command.PromptMessageId
								},
								cancellationToken: cancellationToken
							);

							await commandQueue.DispatchAsync(
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
						await telegramBotClient.SendMessage(
							chatId: command.Chat.Id,
							text: MarkdownV2Sanitizer.Sanitize("Image generation tidak bisa dipakai di sini."),
							parseMode: ParseMode.MarkdownV2,
							replyParameters: new ReplyParameters {
								MessageId = command.PromptMessageId
							},
							cancellationToken: cancellationToken
						);

						break;
				}
			}, logger);

			return default;
		}
	}
}
