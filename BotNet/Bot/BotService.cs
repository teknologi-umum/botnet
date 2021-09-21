using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using Microsoft.ApplicationInsights;
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
	private readonly TelemetryClient _telemetryClient;
	private CancellationTokenSource? _cancellationTokenSource;

	public BotService(
		IClusterClient clusterClient,
		IOptions<BotOptions> optionsAccessor,
		ILogger<BotService> logger,
		TelemetryClient telemetryClient
	) {
		BotOptions options = optionsAccessor.Value;
		if (options.AccessToken is null) throw new InvalidOperationException("Bot access token is not configured. Please add a .NET secret with key 'BotOptions:AccessToken' or a Docker secret with key 'BotOptions__AccessToken'");
		_botClient = new(options.AccessToken);
		_clusterClient = clusterClient;
		_logger = logger;
		_telemetryClient = telemetryClient;
	}

	public Task StartAsync(CancellationToken cancellationToken) {
		_cancellationTokenSource = new();
		_botClient.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), cancellationToken: _cancellationTokenSource.Token);
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
					_logger.LogInformation("Received message from [{firstName} {lastName}]: '{message}' in chat {chatName}.", update.Message!.From!.FirstName, update.Message.From.LastName, update.Message.Text, update.Message.Chat.Title ?? update.Message.Chat.Id.ToString());
					break;
				case UpdateType.InlineQuery:
					_logger.LogInformation("Received inline query from [{firstName} {lastName}]: '{query}'.", update.InlineQuery!.From.FirstName, update.InlineQuery.From.LastName, update.InlineQuery.Query);
					if (update.InlineQuery.Query.Trim().ToLowerInvariant() is { Length: > 0 } query) {
						IInlineQueryGrain inlineQueryGrain = _clusterClient.GetGrain<IInlineQueryGrain>($"{query}|{update.InlineQuery.From.Id}");
						using GrainCancellationTokenSource grainCancellationTokenSource = new();
						using CancellationTokenRegistration tokenRegistration = cancellationToken.Register(() => grainCancellationTokenSource.Cancel());
						IEnumerable<InlineQueryResult> inlineQueryResults = await inlineQueryGrain.GetResultsAsync(query, update.InlineQuery.From.Id, grainCancellationTokenSource.Token);
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
			_logger.LogError(exc, "{message}", exc.Message);
			_telemetryClient.TrackException(exc);
		}
	}

	private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
		string errorMessage = exception switch {
			ApiRequestException apiRequestException => $"Telegram API Error:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
			_ => exception.ToString()
		};
		_logger.LogError(exception, "{message}", errorMessage);
		_telemetryClient.TrackException(exception);
		return Task.CompletedTask;
	}
}
