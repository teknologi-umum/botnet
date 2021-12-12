using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using BotNet.Services.BotCommands;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Orleans;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace BotNet.Bot {
	public class UpdateHandler : IUpdateHandler {
		private readonly IClusterClient _clusterClient;
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<BotService> _logger;
		private readonly TelemetryClient _telemetryClient;
		private User? _me;

		public UpdateHandler(
			IClusterClient clusterClient,
			IServiceProvider serviceProvider,
			ILogger<BotService> logger,
			TelemetryClient telemetryClient
		) {
			_clusterClient = clusterClient;
			_serviceProvider = serviceProvider;
			_logger = logger;
			_telemetryClient = telemetryClient;
		}

		private async Task<User> GetMeAsync(ITelegramBotClient botClient, CancellationToken cancellationToken) {
			if (_me is null) {
				_me = await botClient.GetMeAsync(cancellationToken);
			}
			return _me;
		}

		public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
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
								if (!StringComparer.InvariantCultureIgnoreCase.Equals(targetUsername, (await GetMeAsync(botClient, cancellationToken)).Username)) break;

								// Normalize command
								command = command[..ampersandPos];
							}
							switch (command.ToLowerInvariant()) {
								case "/flip":
									await FlipFlop.HandleFlipAsync(botClient, update.Message, cancellationToken);
									break;
								case "/flop":
									await FlipFlop.HandleFlopAsync(botClient, update.Message, cancellationToken);
									break;
								case "/fuck":
									await Fuck.HandleFuckAsync(botClient, update.Message, cancellationToken);
									break;
								case "/evaljs":
									await Eval.EvalJSAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
								case "/c":
								case "/clojure":
								case "/crystal":
								case "/dart":
								case "/elixir":
								case "/go":
								case "/java":
								case "/kotlin":
								case "/lua":
								case "/pascal":
								case "/php":
								case "/python":
								case "/ruby":
								case "/rust":
								case "/scala":
								case "/swift":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, command.ToLowerInvariant()[1..], cancellationToken);
									break;
								case "/cpp":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "c++", cancellationToken);
									break;
								case "/js":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "javascript", cancellationToken);
									break;
								case "/ts":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "typescript", cancellationToken);
									break;
								case "/pop":
									await botClient.SendTextMessageAsync(
										chatId: update.Message.Chat.Id,
										text: "Here's a bubble wrap. Enjoy!",
										parseMode: ParseMode.Html,
										replyMarkup: Pop.GenerateBubbleWrap(Pop.NewSheet())
									);
									break;
								case "/explain":
									//await OpenAI.ExplainAsync(botClient, _serviceProvider, update.Message, "en", cancellationToken);
									break;
								case "/jelaskan":
									//await OpenAI.ExplainAsync(botClient, _serviceProvider, update.Message, "id", cancellationToken);
									break;
								case "/ask":
									await OpenAI.AskHelpAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
								case "/enid":
								case "/iden":
								case "/eniden":
								case "/idenid":
									await OpenAI.TranslateAsync(botClient, _serviceProvider, update.Message, command.ToLowerInvariant()[1..], cancellationToken);
									break;
								case "/genjs":
									await OpenAI.GenerateJavaScriptCodeAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
								case "/joke":
									await Joke.GetRandomJokeAsync(botClient, _serviceProvider, update.Message, cancellationToken);
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
					case UpdateType.CallbackQuery:
						IBubbleWrapGrain bubbleWrapGrain = _clusterClient.GetGrain<IBubbleWrapGrain>($"{update.CallbackQuery!.Message!.Chat.Id}_{update.CallbackQuery.Message.MessageId}");
						await bubbleWrapGrain.PopAsync(Pop.ParseCallbackData(update.CallbackQuery.Data!));
						bool[,]? data = await bubbleWrapGrain.GetSheetStateAsync();
						await botClient.EditMessageReplyMarkupAsync(
							chatId: update.CallbackQuery!.Message!.Chat.Id,
							messageId: update.CallbackQuery.Message.MessageId,
							replyMarkup: Pop.GenerateBubbleWrap(data!)
						);
						break;
				}
			} catch (OperationCanceledException) {
				throw;
			} catch (Exception exc) {
				_logger.LogError(exc, "{message}", exc.Message);
				_telemetryClient.TrackException(exc);
			}
		}

		public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
			string errorMessage = exception switch {
				ApiRequestException apiRequestException => $"Telegram API Error:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
				_ => exception.ToString()
			};
			_logger.LogError(exception, "{message}", errorMessage);
			_telemetryClient.TrackException(exception);
			return Task.CompletedTask;
		}
	}
}
