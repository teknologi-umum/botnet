using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
						// _logger.LogInformation("Received message from [{firstName} {lastName}]: '{message}' in chat {chatName}.", update.Message!.From!.FirstName, update.Message.From.LastName, update.Message.Text, update.Message.Chat.Title ?? update.Message.Chat.Id.ToString());

						// Retrieve bot identity
						_me ??= await GetMeAsync(botClient, cancellationToken);

						// Handle call sign
						if (update.Message?.Text is { } messageText && (
							messageText.StartsWith("AI,")
						// || messageText.StartsWith("Pakde,")
						)) {
							// Get call sign
							string callSign = messageText.Split(',')[0];

							// Respond to call sign
							Message? sentMessage = callSign switch {
								"AI" => await OpenAI.ChatWithFriendlyBotAsync(botClient, _serviceProvider, update.Message, callSign, cancellationToken),
								// "Pakde" => await OpenAI.ChatWithSarcasticBotAsync(botClient, _serviceProvider, update.Message, callSign, cancellationToken),
								_ => throw new NotImplementedException($"Call sign {callSign} not handled")
							};

							if (sentMessage is not null) {
								// Track sent message
								await _clusterClient.GetGrain<ITrackedMessageGrain>(sentMessage.MessageId).TrackMessageAsync(
									sender: callSign,
									text: sentMessage.Text!,
									replyToMessageId: sentMessage.ReplyToMessage!.MessageId
								);
							}
							break;
						}

						// Handle reply
						if (update.Message is {
							MessageId: int messageId,
							From: { FirstName: string firstName, LastName: var lastName },
							Text: { Length: > 0 } text
						}
							&& update.Message.Entities?.FirstOrDefault(entity => entity is { Type: MessageEntityType.BotCommand, Offset: 0 }) is null
							&& update.Message.ReplyToMessage is {
								MessageId: int replyToMessageId,
								From: { Id: long replyToUserId }
							}
							&& replyToUserId == _me?.Id) {

							// Track message
							await _clusterClient.GetGrain<ITrackedMessageGrain>(update.Message.MessageId).TrackMessageAsync(
								sender: $"{firstName}{lastName?.Let(lastName => " " + lastName)}",
								text: text,
								replyToMessageId: replyToMessageId
							);

							// Get thread
							ImmutableList<(string Sender, string Text)> thread = await _clusterClient.GetGrain<ITrackedMessageGrain>(replyToMessageId).GetThreadAsync(maxLines: 20);

							// Don't respond if thread is empty
							if (thread.Count > 0) {
								// Identify last AI in thread
								string callSign = thread.Last().Sender;

								// Respond to thread
								Message? sentMessage = callSign switch {
									"AI" => await OpenAI.ChatWithFriendlyBotAsync(botClient, _serviceProvider, update.Message, thread, callSign, cancellationToken),
									"Pakde" => await OpenAI.ChatWithSarcasticBotAsync(botClient, _serviceProvider, update.Message, thread, callSign, cancellationToken),
									_ => throw new NotImplementedException($"Call sign {callSign} not handled")
								};

								if (sentMessage is not null) {
									// Track sent message
									await _clusterClient.GetGrain<ITrackedMessageGrain>(sentMessage.MessageId).TrackMessageAsync(
										sender: callSign,
										text: sentMessage.Text!,
										replyToMessageId: sentMessage.ReplyToMessage!.MessageId
									);
								}
								break;
							}
						}

						// Handle commands
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
								case "/evalcs":
									await Eval.EvalCSAsync(botClient, _serviceProvider, update.Message, cancellationToken);
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
								case "/cs":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "csharp.net", cancellationToken);
									break;
								case "/fs":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "fsharp.net", cancellationToken);
									break;
								case "/js":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "javascript", cancellationToken);
									break;
								case "/ts":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "typescript", cancellationToken);
									break;
								case "/vb":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "basic.net", cancellationToken);
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
								case "/humor":
									await Joke.GetRandomJokeAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
								case "/clean":
									await Clean.SanitizeLinkAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
								case "/waifu":
									await Waifu.GetRandomWaifuAsync(botClient, update.Message, cancellationToken);
									break;
								case "/cat":
									await Cat.GetRandomCatAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
								case "/idea":
									await Idea.GetRandomIdeaAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
								case "/art":
									await Art.GetRandomArtAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
								case "/tldr":
									await OpenAI.GenerateTldrAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
								case "/webp":
									await Webp.ConvertToImageAsync(botClient, update.Message, cancellationToken);
									break;
								case "/pse":
									await Services.BotCommands.PSE.SearchAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
							}
						}
						break;
					case UpdateType.InlineQuery:
						// _logger.LogInformation("Received inline query from [{firstName} {lastName}]: '{query}'.", update.InlineQuery!.From.FirstName, update.InlineQuery.From.LastName, update.InlineQuery.Query);
						if (update.InlineQuery?.Query.Trim().ToLowerInvariant() is { Length: > 0 } query) {
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
					default:
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
