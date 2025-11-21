using BotNet.Commands.Mermaid;
using BotNet.Services.Mermaid;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Mermaid {
	public sealed class MermaidCommandHandler(
		ITelegramBotClient telegramBotClient,
		MermaidRenderer mermaidRenderer,
		ILogger<MermaidCommandHandler> logger
	) : ICommandHandler<MermaidCommand> {
		internal static readonly RateLimiter MermaidRateLimiter = RateLimiter.PerUserPerChat(5, TimeSpan.FromMinutes(5));

		public async Task Handle(MermaidCommand command, CancellationToken cancellationToken) {
			try {
				MermaidRateLimiter.ValidateActionRate(command.Chat.Id, command.Sender.Id);
			} catch (RateLimitExceededException exc) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"Anda belum mendapat giliran. Coba lagi {exc.Cooldown}.",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters {
						MessageId = command.CommandMessageId
					},
					cancellationToken: cancellationToken
				);
				return;
			}

			// Fire and forget
			BackgroundTask.Run(async () => {
				Message busyMessage = await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: "Generating mermaid diagram… ⏳",
					parseMode: ParseMode.Markdown,
					replyParameters: new ReplyParameters {
						MessageId = command.CommandMessageId
					},
					cancellationToken: cancellationToken
				);

				try {
					byte[] diagramImage = await mermaidRenderer.RenderMermaidAsync(command.MermaidCode, cancellationToken);

					using MemoryStream stream = new(diagramImage);
					await telegramBotClient.SendPhoto(
						chatId: command.Chat.Id,
						photo: new InputFileStream(stream, "mermaid.png"),
						caption: null,
						replyParameters: new ReplyParameters {
							MessageId = command.CommandMessageId
						},
						cancellationToken: cancellationToken
					);

					// Delete the busy message
					await telegramBotClient.DeleteMessage(
						chatId: command.Chat.Id,
						messageId: busyMessage.MessageId,
						cancellationToken: cancellationToken
					);
				} catch (MermaidRenderException ex) {
					await telegramBotClient.EditMessageText(
						chatId: command.Chat.Id,
						messageId: busyMessage.MessageId,
						text: $"Error: Invalid mermaid code - {ex.Message}",
						cancellationToken: cancellationToken
					);
				} catch (Exception ex) {
					await telegramBotClient.EditMessageText(
						chatId: command.Chat.Id,
						messageId: busyMessage.MessageId,
						text: $"Error generating diagram: {ex.Message}",
						cancellationToken: cancellationToken
					);
				}
			}, logger);
		}
	}
}
