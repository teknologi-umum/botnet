using BotNet.Services.Giphy;
using BotNet.Services.Giphy.Models;
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
	private readonly GiphyClient _giphyClient;
	private readonly ILogger<BotService> _logger;
	private CancellationTokenSource? _cancellationTokenSource;

	public BotService(
		GiphyClient giphyClient,
		IOptions<BotOptions> optionsAccessor,
		ILogger<BotService> logger
	) {
		BotOptions options = optionsAccessor.Value;
		if (options.AccessToken is null) throw new InvalidOperationException("Bot access token is not configured.");
		_botClient = new(options.AccessToken);
		_giphyClient = giphyClient;
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
							GifObject[] gifObjects = await _giphyClient.SearchGifsAsync(query, cancellationToken);
							await botClient.AnswerInlineQueryAsync(update.InlineQuery.Id, gifObjects.Select(gifObject => new InlineQueryResultGif(
								id: gifObject.Id,
								gifUrl: gifObject.Url,
								thumbUrl: gifObject.Url
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
