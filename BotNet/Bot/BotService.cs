using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace BotNet.Bot;

public class BotService : IHostedService {
	private readonly TelegramBotClient _botClient;
	private readonly IClusterClient _clusterClient;
	private readonly ILogger<BotService> _logger;
	private CancellationTokenSource? _cancellationTokenSource;

	public BotService(
		IClusterClient clusterClient,
		IOptions<BotOptions> optionsAccessor,
		ILogger<BotService> logger
	) {
		BotOptions options = optionsAccessor.Value;
		if (options.AccessToken is null) throw new InvalidOperationException("Bot access token is not configured. Please add a .NET secret with key 'BotOptions:AccessToken' or a Docker secret with key 'BotOptions__AccessToken'");
		_botClient = new(options.AccessToken);
		_clusterClient = clusterClient;
		_logger = logger;
	}

	public Task StartAsync(CancellationToken cancellationToken) {
		_cancellationTokenSource = new();
		_botClient.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), _cancellationTokenSource.Token);
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken) {
		_cancellationTokenSource?.Cancel();
		return Task.CompletedTask;
	}

	private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
		try {
			switch (update.Type) {
				case UpdateType.Message:
					_logger.LogInformation($"Received message from [{update.Message.From.FirstName} {update.Message.From.LastName}]: '{update.Message.Text}' in chat {update.Message.Chat.Title ?? update.Message.Chat.Id.ToString()}.");
					break;
				case UpdateType.InlineQuery:
					_logger.LogInformation($"Received inline query from [{update.InlineQuery.From.FirstName} {update.InlineQuery.From.LastName}]: '{update.InlineQuery.Query}'.");
					if (update.InlineQuery.Query.Trim().ToLowerInvariant() is { Length: > 0 } query) {
						IInlineQueryGrain inlineQueryGrain = _clusterClient.GetGrain<IInlineQueryGrain>(query);
						IEnumerable<InlineQueryResultBase> inlineQueryResults = await inlineQueryGrain.GetResultsAsync(update.InlineQuery);
						await botClient.AnswerInlineQueryAsync(
							inlineQueryId: update.InlineQuery.Id,
							results: inlineQueryResults,
							cancellationToken: cancellationToken);
					}
					break;
			}
		} catch (OperationCanceledException) {
			throw;
		} catch (Exception exc) {
			_logger.LogError(exc, exc.Message);
		}
	}

	private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
		string errorMessage = exception switch {
			ApiRequestException apiRequestException => $"Telegram API Error:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
			_ => exception.ToString()
		};
		_logger.LogError(exception, errorMessage);
		return Task.CompletedTask;
	}
}
