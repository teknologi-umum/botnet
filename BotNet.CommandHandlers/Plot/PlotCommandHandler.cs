using BotNet.Commands.Plot;
using Mediator;
using BotNet.Services.Plot;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Plot {
	public sealed class PlotCommandHandler(
		ITelegramBotClient telegramBotClient,
		MathPlotRenderer mathPlotRenderer,
		ILogger<PlotCommandHandler> logger
	) : ICommandHandler<PlotCommand> {
		internal static readonly RateLimiter PlotRateLimiter = RateLimiter.PerUserPerChat(5, TimeSpan.FromMinutes(5));

		public async ValueTask<Unit> Handle(PlotCommand command, CancellationToken cancellationToken) {
			try {
				PlotRateLimiter.ValidateActionRate(command.Chat.Id, command.Sender.Id);
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
				return default;
			}

			// Fire and forget
			BackgroundTask.Run(async () => {
				Message busyMessage = await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: "Generating plot… ⏳",
					parseMode: ParseMode.Markdown,
					replyParameters: new ReplyParameters {
						MessageId = command.CommandMessageId
					},
					cancellationToken: cancellationToken
				);

				try {
					byte[] plotImage = mathPlotRenderer.RenderPlot(command.Expression);

					using MemoryStream stream = new(plotImage);
					await telegramBotClient.SendPhoto(
						chatId: command.Chat.Id,
						photo: new InputFileStream(stream, "plot.png"),
						caption: command.Expression,
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
				} catch (Exception ex) {
					await telegramBotClient.EditMessageText(
						chatId: command.Chat.Id,
						messageId: busyMessage.MessageId,
						text: $"Error generating plot: {ex.Message}",
						cancellationToken: cancellationToken
					);
				}
			}, logger);
	return default;
		}
	}
}
