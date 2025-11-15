using System;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Commands.BotUpdate.CallbackQuery;
using BotNet.Commands.BotUpdate.InlineQuery;
using BotNet.Commands.BotUpdate.Message;
using MediatR;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Bot {
	public class UpdateHandler(
		IMediator mediator,
		ILogger<BotService> logger
	) : IUpdateHandler {
		public async Task HandleUpdateAsync(
			ITelegramBotClient botClient,
			Update update,
			CancellationToken cancellationToken
		) {
			try {
				switch (update.Type) {
					case UpdateType.Message:
						await mediator.Send(new MessageUpdate(update.Message!), cancellationToken);
						break;
					case UpdateType.InlineQuery:
						await mediator.Send(new InlineQueryUpdate(update.InlineQuery!), cancellationToken);
						break;
					case UpdateType.CallbackQuery:
						await mediator.Send(new CallbackQueryUpdate(update.CallbackQuery!), cancellationToken);
						break;
				}
			} catch (OperationCanceledException) {
				throw;
			} catch (Exception exc) {
				logger.LogError(exc, "{message}", exc.Message);
			}
		}

		public Task HandleErrorAsync(
			ITelegramBotClient botClient,
			Exception exception,
			HandleErrorSource source,
			CancellationToken cancellationToken
		) {
			string errorMessage = exception switch {
				ApiRequestException apiRequestException =>
					$"Telegram API Error:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
				_ => exception.ToString()
			};
			logger.LogError(exception, "{message}", errorMessage);
			return Task.CompletedTask;
		}
	}
}
