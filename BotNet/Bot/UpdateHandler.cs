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
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Bot {
	public class UpdateHandler(
		IMediator mediator,
		ILogger<BotService> logger
	) : IUpdateHandler {
		private readonly IMediator _mediator = mediator;
		private readonly ILogger<BotService> _logger = logger;

		public async Task HandleUpdateAsync(
			ITelegramBotClient botClient,
			Update update,
			CancellationToken cancellationToken
		) {
			try {
				switch (update.Type) {
					case UpdateType.Message:
						await _mediator.Send(new MessageUpdate(update.Message!));
						break;
					case UpdateType.InlineQuery:
						await _mediator.Send(new InlineQueryUpdate(update.InlineQuery!));
						break;
					case UpdateType.CallbackQuery:
						await _mediator.Send(new CallbackQueryUpdate(update.CallbackQuery!));
						break;
					default:
						break;
				}
			} catch (OperationCanceledException) {
				throw;
			} catch (Exception exc) {
				_logger.LogError(exc, "{message}", exc.Message);
			}
		}

		public Task HandleErrorAsync(
			ITelegramBotClient botClient,
			Exception exception,
			CancellationToken cancellationToken
		) {
			string errorMessage = exception switch {
				ApiRequestException apiRequestException =>
					$"Telegram API Error:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
				_ => exception.ToString()
			};
			_logger.LogError(exception, "{message}", errorMessage);
			return Task.CompletedTask;
		}
	}
}
