using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Tenor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace BotNet.Bot;

public class BotService : IHostedService {
	private readonly TelegramBotClient _botClient;
	private readonly TenorClient _tenorClient;
	private readonly ILogger<BotService> _logger;
	private CancellationTokenSource? _cancellationTokenSource;

	public BotService(
		TenorClient tenorClient,
		IOptions<BotOptions> optionsAccessor,
		ILogger<BotService> logger
	) {
		BotOptions options = optionsAccessor.Value;
		if (options.AccessToken is null) throw new InvalidOperationException("Bot access token is not configured. Please add a .NET secret with key 'BotOptions:AccessToken' or a Docker secret with key 'BotOptions__AccessToken'");
		_botClient = new(options.AccessToken);
		_tenorClient = tenorClient;
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
		switch (update.Type) {
			case UpdateType.Message:
				_logger.LogInformation($"Received message from [{update.Message.From.FirstName} {update.Message.From.LastName}]: '{update.Message.Text}' in chat {update.Message.Chat.Id}.");
				break;
			case UpdateType.InlineQuery:
				switch (update.InlineQuery.Query.Trim().ToLowerInvariant()) {
					case "illuminati":
						await botClient.AnswerInlineQueryAsync(update.InlineQuery.Id, new List<InlineQueryResultGif> {
							new InlineQueryResultGif("illuminati1", "https://media.giphy.com/media/uFOW5cbNaoTaU/giphy.gif", "https://media.giphy.com/media/uFOW5cbNaoTaU/giphy.gif"),
							new InlineQueryResultGif("illuminati2", "https://media4.giphy.com/media/ZTfTSegFNMnC0/giphy.gif", "https://media4.giphy.com/media/ZTfTSegFNMnC0/giphy.gif")
						}, cancellationToken: cancellationToken);
						break;
					case string { Length: >= 3 } query: {
							(string Id, string Url, string PreviewUrl)[] gifs = await _tenorClient.SearchGifsAsync(query, cancellationToken);
							await botClient.AnswerInlineQueryAsync(update.InlineQuery.Id, gifs.Select(gif => new InlineQueryResultGif(
								id: gif.Id,
								gifUrl: gif.Url,
								thumbUrl: gif.PreviewUrl
							)).ToList(), cancellationToken: cancellationToken);
							break;
						}
						
				}
				_logger.LogInformation($"Received inline query from [{update.InlineQuery.From.FirstName} {update.InlineQuery.From.LastName}]: '{update.InlineQuery.Query}'.");
				break;
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
