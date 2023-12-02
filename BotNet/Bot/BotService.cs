using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotNet.Bot;

public class BotService(
	ITelegramBotClient telegramBotClient,
	IOptions<BotOptions> botOptionsAccessor,
	IOptions<HostingOptions> hostingOptionsAccessor,
	UpdateHandler updateHandler
) : IHostedService {
	private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
	private readonly UpdateHandler _updateHandler = updateHandler;
	private readonly string _botToken = botOptionsAccessor.Value.AccessToken!;
	private readonly string _hostName = hostingOptionsAccessor.Value.HostName!;
	private readonly bool _useLongPolling = hostingOptionsAccessor.Value.UseLongPolling;
	private CancellationTokenSource? _cancellationTokenSource;

	public Task StartAsync(CancellationToken cancellationToken) {
		_cancellationTokenSource = new();
		if (_useLongPolling) {
			_telegramBotClient.StartReceiving(_updateHandler, cancellationToken: _cancellationTokenSource.Token);
			return Task.CompletedTask;
		} else {
			string webhookAddress = $"https://{_hostName}/webhook/{_botToken.Split(':')[1]}";
			return _telegramBotClient.SetWebhookAsync(
				url: webhookAddress,
				allowedUpdates: new[] {
					UpdateType.CallbackQuery,
					UpdateType.InlineQuery,
					UpdateType.Message,
				},
				cancellationToken: cancellationToken
			);
		}
	}

	public Task StopAsync(CancellationToken cancellationToken) {
		_cancellationTokenSource?.Cancel();
		if (_useLongPolling) {
			return _telegramBotClient.CloseAsync(cancellationToken);
		} else {
			return _telegramBotClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
		}
	}
}
