using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.BotCommands;
using BotNet.Services.BubbleWrap;
using BotNet.Services.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RG.Ninja;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotNet.Bot {
	public class UpdateHandler(
		IServiceProvider serviceProvider,
		ILogger<BotService> logger,
		InlineQueryHandler inlineQueryHandler
	) : IUpdateHandler {
		private readonly IServiceProvider _serviceProvider = serviceProvider;
		private readonly ILogger<BotService> _logger = logger;
		private readonly InlineQueryHandler _inlineQueryHandler = inlineQueryHandler;
		private User? _me;

		private async Task<User> GetMeAsync(ITelegramBotClient botClient, CancellationToken cancellationToken) {
			_me ??= await botClient.GetMeAsync(cancellationToken);
			return _me;
		}

		public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
			try {
				switch (update.Type) {
					case UpdateType.Message:
						// _logger.LogInformation("Received message from [{firstName} {lastName}]: '{message}' in chat {chatName}.", update.Message!.From!.FirstName, update.Message.From.LastName, update.Message.Text, update.Message.Chat.Title ?? update.Message.Chat.Id.ToString());

						// Retrieve bot identity
						_me ??= await GetMeAsync(botClient, cancellationToken);

						// Handle reddit mirroring
						if (update.Message?.Entities?.FirstOrDefault(entity => entity is { Type: MessageEntityType.Url }) is { Offset: var offset, Length: var length }
							&& update.Message.Text?.Substring(offset, length) is { } url
							&& url.StartsWith("https://www.reddit.com/", out string? remainingUrl)) {
							await botClient.SendTextMessageAsync(
								chatId: update.Message.Chat.Id,
								text: $"Mirror: https://libreddit.teknologiumum.com/{remainingUrl}",
								replyToMessageId: update.Message.MessageId,
								disableWebPagePreview: true,
								cancellationToken: cancellationToken
							);
						} else if (update.Message?.Entities?.FirstOrDefault(entity => entity is { Type: MessageEntityType.TextLink }) is { Url: { } textUrl }
							&& textUrl.StartsWith("https://www.reddit.com/", out string? remainingTextUrl)) {
							await botClient.SendTextMessageAsync(
								chatId: update.Message.Chat.Id,
								text: $"Mirror: https://libreddit.teknologiumum.com/{remainingTextUrl}",
								replyToMessageId: update.Message.MessageId,
								disableWebPagePreview: true,
								cancellationToken: cancellationToken
							);
						}

						// Handle call sign
						if (update.Message?.Text is { } messageText && (
							messageText.StartsWith("AI,")
							|| messageText.StartsWith("Pakde,")
						)) {
							// Get call sign
							string callSign = messageText.Split(',')[0];

							// Handle modify art command
							if (callSign == "AI" && (update.Message.ReplyToMessage is { Photo.Length: > 0 } || update.Message.ReplyToMessage is { Sticker: { } })) {
								await Art.ModifyArtAsync(botClient, _serviceProvider, update.Message, messageText[(callSign.Length + 2)..], cancellationToken);
								break;
							}

							// Respond to call sign
							switch (callSign) {
								case "AI":
									await OpenAI.StreamChatWithFriendlyBotAsync(botClient, _serviceProvider, "AI", update.Message, cancellationToken);
									break;
								case "Pakde":
									Message? sentMessage = await OpenAI.ChatWithSarcasticBotAsync(botClient, _serviceProvider, update.Message, callSign, cancellationToken);
									if (sentMessage is not null) {
										// Track sent message
										_serviceProvider.GetRequiredService<ThreadTracker>().TrackMessage(
											messageId: sentMessage.MessageId,
											sender: callSign,
											text: sentMessage.Text!,
											replyToMessageId: sentMessage.ReplyToMessage!.MessageId
										);
									}
									break;
								default:
									throw new NotImplementedException($"Call sign {callSign} not handled");
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
								From.Id: long replyToUserId
							}
							&& replyToUserId == _me?.Id) {

							ThreadTracker threadTracker = _serviceProvider.GetRequiredService<ThreadTracker>();

							// Track message
							threadTracker.TrackMessage(
								messageId: update.Message.MessageId,
								sender: $"{firstName}{lastName?.Let(lastName => " " + lastName)}",
								text: text,
								replyToMessageId: replyToMessageId
							);

							// Get thread
							ImmutableList<(string Sender, string Text)> thread = threadTracker.GetThread(
								messageId: replyToMessageId,
								maxLines: 20
							).ToImmutableList();

							// Don't respond if thread is empty
							if (thread.Count > 0) {
								// Identify last AI in thread
								string callSign = thread.Last().Sender;

								// Respond to thread
								switch (callSign) {
									case "AI":
										await OpenAI.StreamChatWithFriendlyBotAsync(botClient, _serviceProvider, "AI", update.Message, thread, cancellationToken);
										break;
									case "Pakde":
										Message? sentMessage = await OpenAI.ChatWithSarcasticBotAsync(botClient, _serviceProvider, update.Message, thread, callSign, cancellationToken);
										if (sentMessage is not null) {
											// Track sent message
											threadTracker.TrackMessage(
												messageId: sentMessage.MessageId,
												sender: callSign,
												text: sentMessage.Text!,
												replyToMessageId: sentMessage.ReplyToMessage!.MessageId
											);
										}
										break;
									default:
										throw new NotImplementedException($"Call sign {callSign} not handled");
								}
								break;
							}
						}

						// Handle commands
						if (update.Message?.Entities?.FirstOrDefault(entity => entity is { Type: MessageEntityType.BotCommand, Offset: 0 }) is { } commandEntity) {
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
								case "/flap":
									await FlipFlop.HandleFlapAsync(botClient, update.Message, cancellationToken);
									break;
								case "/flep":
									await FlipFlop.HandleFlepAsync(botClient, update.Message, cancellationToken);
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
								case "/julia":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, command.ToLowerInvariant()[1..], command.ToLowerInvariant()[1..], cancellationToken);
									break;
								case "/sqlite3":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "SQLite3", "sqlite3", cancellationToken);
									break;
								case "/commonlisp":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "CommonLisp", "cl", cancellationToken);
									break;
								case "/cpp":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "C++", "cpp", cancellationToken);
									break;
								case "/cs":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "csharp.net", "csharp", cancellationToken);
									break;
								case "/fs":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "fsharp.net", "fsharp", cancellationToken);
									break;
								case "/js":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "JavaScript", "js", cancellationToken);
									break;
								case "/ts":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "TypeScript", "ts", cancellationToken);
									break;
								case "/vb":
									await Exec.ExecAsync(botClient, _serviceProvider, update.Message, "basic.net", "vbnet", cancellationToken);
									break;
								case "/pop":
									await botClient.SendTextMessageAsync(
										chatId: update.Message.Chat.Id,
										text: "Here's a bubble wrap. Enjoy!",
										parseMode: ParseMode.Html,
										replyMarkup: BubbleWrapKeyboardGenerator.EMPTY_KEYBOARD,
										cancellationToken: cancellationToken
									);
									break;
								case "/explain":
									await OpenAI.ExplainAsync(botClient, _serviceProvider, update.Message, "en", cancellationToken);
									break;
								case "/jelaskan":
									await OpenAI.ExplainAsync(botClient, _serviceProvider, update.Message, "id", cancellationToken);
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
								case "/map":
									await SearchPlace.SearchPlaceAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
								case "/weather":
									await Weather.GetWeatherAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
								case "/bmkg":
									await BMKG.GetLatestEarthQuakeAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
								case "/preview":
									await Preview.GetPreviewAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
								case "/ramad":
									await Meme.HandleRamadAsync(botClient, _serviceProvider, update.Message, cancellationToken);
									break;
							}
						}
						break;
					case UpdateType.InlineQuery:
						if (update.InlineQuery?.Query.Trim().ToLowerInvariant() is { Length: > 0 } query) {
							IEnumerable<InlineQueryResult> inlineQueryResults = await _inlineQueryHandler.GetResultsAsync(query, cancellationToken);
							await botClient.AnswerInlineQueryAsync(
								inlineQueryId: update.InlineQuery.Id,
								results: inlineQueryResults,
								cancellationToken: cancellationToken);
						}
						break;
					case UpdateType.CallbackQuery:
						BubbleWrapKeyboardGenerator bubbleWrapKeyboardGenerator = _serviceProvider.GetRequiredService<BubbleWrapKeyboardGenerator>();
						InlineKeyboardMarkup poppedKeyboardMarkup = bubbleWrapKeyboardGenerator.HandleCallback(
							chatId: update.CallbackQuery!.Message!.Chat.Id,
							messageId: update.CallbackQuery.Message.MessageId,
							callbackData: update.CallbackQuery.Data!
						);
						await botClient.EditMessageReplyMarkupAsync(
							chatId: update.CallbackQuery!.Message!.Chat.Id,
							messageId: update.CallbackQuery.Message.MessageId,
							replyMarkup: poppedKeyboardMarkup,
							cancellationToken: cancellationToken
						);
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

		public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
			string errorMessage = exception switch {
				ApiRequestException apiRequestException => $"Telegram API Error:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
				_ => exception.ToString()
			};
			_logger.LogError(exception, "{message}", errorMessage);
			return Task.CompletedTask;
		}
	}
}
