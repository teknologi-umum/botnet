using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Bot;

public class BotService : IHostedService {
	private readonly TelegramBotClient _botClient;
	private readonly ILogger<BotService> _logger;
	private CancellationTokenSource? _cancellationTokenSource;

	public BotService(
		IOptions<BotOptions> optionsAccessor,
		ILogger<BotService> logger
	) {
		BotOptions options = optionsAccessor.Value;
		if (options.AccessToken is null) throw new InvalidOperationException("Bot access token is not configured.");
		_botClient = new(options.AccessToken);
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

	private Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
		if (update.Type != UpdateType.Message)
			return Task.CompletedTask;
		if (update.Message.Type != MessageType.Text)
			return Task.CompletedTask;
		long chatId = update.Message.Chat.Id;
		_logger.LogInformation($"Received message from [{update.Message.From.FirstName} {update.Message.From.LastName}]: '{update.Message.Text}' in chat {chatId}.");
		return Task.CompletedTask;
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
