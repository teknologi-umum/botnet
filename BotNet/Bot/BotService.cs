using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using BotNet.Services.ImageFlip;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
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
using Telegram.Bot.Types.InputFiles;

namespace BotNet.Bot;

public class BotService : IHostedService {
	private readonly TelegramBotClient _botClient;
	private readonly IClusterClient _clusterClient;
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<BotService> _logger;
	private readonly TelemetryClient _telemetryClient;
	private User? _me;
	private CancellationTokenSource? _cancellationTokenSource;

	public BotService(
		IClusterClient clusterClient,
		IServiceProvider serviceProvider,
		IOptions<BotOptions> optionsAccessor,
		ILogger<BotService> logger,
		TelemetryClient telemetryClient
	) {
		BotOptions options = optionsAccessor.Value;
		if (options.AccessToken is null) throw new InvalidOperationException("Bot access token is not configured. Please add a .NET secret with key 'BotOptions:AccessToken' or a Docker secret with key 'BotOptions__AccessToken'");
		_botClient = new(options.AccessToken);
		_clusterClient = clusterClient;
		_serviceProvider = serviceProvider;
		_logger = logger;
		_telemetryClient = telemetryClient;
	}

	public async Task StartAsync(CancellationToken cancellationToken) {
		_cancellationTokenSource = new();
		_me = await _botClient.GetMeAsync(cancellationToken);
		_botClient.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), cancellationToken: _cancellationTokenSource.Token);
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
					if (update.Message.Entities?.FirstOrDefault(entity => entity is { Type: MessageEntityType.BotCommand, Offset: 0 }) is { } commandEntity) {
						string command = update.Message.Text!.Substring(commandEntity.Offset, commandEntity.Length);

						// Check if command is in /command@botname format
						int ampersandPos = command.IndexOf('@');
						if (ampersandPos != -1) {
							string targetUsername = command[(ampersandPos + 1)..];

							// Command is not for me
							if (!StringComparer.InvariantCultureIgnoreCase.Equals(targetUsername, _me?.Username)) break;

							// Normalize command
							command = command[..ampersandPos];
						}
						switch (command.ToLowerInvariant()) {
							case "/flip":
								await HandleFlipAsync(botClient, update.Message, cancellationToken);
								break;
							case "/flop":
								await HandleFlopAsync(botClient, update.Message, cancellationToken);
								break;
						}
					}
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

	private static async Task HandleFlipAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
		if (message.ReplyToMessage is null) {
			await botClient.SendTextMessageAsync(
				chatId: message.Chat.Id,
				text: "Apa yang mau diflip? Untuk ngeflip gambar, reply `/flip` ke pesan yang ada gambarnya\\.",
				parseMode: ParseMode.MarkdownV2,
				replyToMessageId: message.MessageId,
				cancellationToken: cancellationToken);
		} else if (message.ReplyToMessage.Photo is null || message.ReplyToMessage.Photo.Length == 0) {
			await botClient.SendTextMessageAsync(
				chatId: message.Chat.Id,
				text: "Pesan ini tidak ada gambarnya\\. Untuk ngeflip gambar, reply `/flip` ke pesan yang ada gambarnya\\.",
				parseMode: ParseMode.MarkdownV2,
				replyToMessageId: message.MessageId,
				cancellationToken: cancellationToken);
		} else {
			using MemoryStream originalImageStream = new();
			Telegram.Bot.Types.File fileInfo = await botClient.GetInfoAndDownloadFileAsync(
				fileId: message.ReplyToMessage.Photo.OrderByDescending(photoSize => photoSize.Width).First().FileId,
				destination: originalImageStream,
				cancellationToken: cancellationToken);

			byte[] flippedImage = Flipper.FlipImage(originalImageStream.ToArray());
			using MemoryStream flippedImageStream = new(flippedImage);

			await botClient.SendPhotoAsync(
				chatId: message.Chat.Id,
				photo: new InputOnlineFile(flippedImageStream, new string(fileInfo.FileId.Reverse().ToArray()) + ".png"),
				replyToMessageId: message.ReplyToMessage.MessageId);
		}
	}

	private static async Task HandleFlopAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
		if (message.ReplyToMessage is null) {
			await botClient.SendTextMessageAsync(
				chatId: message.Chat.Id,
				text: "Apa yang mau diflop? Untuk ngeflop gambar, reply `/flop` ke pesan yang ada gambarnya\\.",
				parseMode: ParseMode.MarkdownV2,
				replyToMessageId: message.MessageId,
				cancellationToken: cancellationToken);
		} else if (message.ReplyToMessage.Photo is null || message.ReplyToMessage.Photo.Length == 0) {
			await botClient.SendTextMessageAsync(
				chatId: message.Chat.Id,
				text: "Pesan ini tidak ada gambarnya\\. Untuk ngeflop gambar, reply `/flop` ke pesan yang ada gambarnya\\.",
				parseMode: ParseMode.MarkdownV2,
				replyToMessageId: message.MessageId,
				cancellationToken: cancellationToken);
		} else {
			using MemoryStream originalImageStream = new();
			Telegram.Bot.Types.File fileInfo = await botClient.GetInfoAndDownloadFileAsync(
				fileId: message.ReplyToMessage.Photo.OrderByDescending(photoSize => photoSize.Width).First().FileId,
				destination: originalImageStream,
				cancellationToken: cancellationToken);

			byte[] floppedImage = Flipper.FlopImage(originalImageStream.ToArray());
			using MemoryStream flippedImageStream = new(floppedImage);

			await botClient.SendPhotoAsync(
				chatId: message.Chat.Id,
				photo: new InputOnlineFile(flippedImageStream, new string(fileInfo.FileId.Reverse().ToArray()) + ".png"),
				replyToMessageId: message.ReplyToMessage.MessageId);
		}
	}
}
