﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Commands;
using BotNet.Commands.BotUpdate.CallbackQuery;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Services.BotCommands;
using BotNet.Services.OpenAI;
using BotNet.Services.SocialLink;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RG.Ninja;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace BotNet.Bot {
	public class UpdateHandler(
		IMediator mediator,
		IServiceProvider serviceProvider,
		ILogger<BotService> logger,
		InlineQueryHandler inlineQueryHandler
	) : IUpdateHandler {
		private readonly IMediator _mediator = mediator;
		private readonly IServiceProvider _serviceProvider = serviceProvider;
		private readonly ILogger<BotService> _logger = logger;
		private readonly InlineQueryHandler _inlineQueryHandler = inlineQueryHandler;
		private User? _me;

		private async Task<User> GetMeAsync(ITelegramBotClient botClient, CancellationToken cancellationToken) {
			_me ??= await botClient.GetMeAsync(cancellationToken);
			return _me;
		}

		public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
			CancellationToken cancellationToken) {
			try {
				switch (update.Type) {
					case UpdateType.Message:
						// _logger.LogInformation("Received message from [{firstName} {lastName}]: '{message}' in chat {chatName}.", update.Message!.From!.FirstName, update.Message.From.LastName, update.Message.Text, update.Message.Chat.Title ?? update.Message.Chat.Id.ToString());

						// Retrieve bot identity
						_me ??= await GetMeAsync(botClient, cancellationToken);

						// Handle Social Link (better embed) using SocialLinkEmbedFixer 
						if (update.Message != null) {
							string message = update.Message.Text ?? update.Message.Caption ?? "";
							IEnumerable<Uri> possibleUrls = SocialLinkEmbedFixer.GetPossibleUrls(message);

							foreach (Uri u in possibleUrls.Select(SocialLinkEmbedFixer.Fix)) {
								await botClient.SendTextMessageAsync(
									chatId: update.Message.Chat.Id,
									text: $"Preview: {u.OriginalString}",
									replyToMessageId: update.Message.MessageId,
									cancellationToken: cancellationToken);
							}
						}

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
						} else if (update.Message?.Entities?.FirstOrDefault(entity =>
									   entity is { Type: MessageEntityType.TextLink }) is { Url: { } textUrl }
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
						if ((update.Message?.Text ?? update.Message?.Caption) is { } messageText && (
							messageText.StartsWith("AI,")
							|| messageText.StartsWith("Pakde,")
						)) {
							// Get call sign
							string callSign = messageText.Split(',')[0];

							// Handle modify art command
							//if (callSign == "AI" && (update.Message.ReplyToMessage is { Photo.Length: > 0 } || update.Message.ReplyToMessage is { Sticker: { } })) {
							//	await Art.ModifyArtAsync(botClient, _serviceProvider, update.Message, messageText[(callSign.Length + 2)..], cancellationToken);
							//	break;
							//}

							// Respond to call sign
							switch (callSign) {
								case "AI":
									// Try merging with previous thread
									if (update.Message!.ReplyToMessage is { Text: { } replyToText, From: { } replyToFrom } replyToMessage) {
										ThreadTracker threadTracker = _serviceProvider.GetRequiredService<ThreadTracker>();
										threadTracker.TrackMessage(
											messageId: replyToMessage.MessageId,
											sender: $"{replyToFrom.FirstName}{replyToFrom.LastName?.Let(lastName => " " + lastName)}",
											text: replyToText,
											imageBase64: null,
											replyToMessageId: replyToMessage.ReplyToMessage?.MessageId
										);
										threadTracker.TrackMessage(
											messageId: update.Message.MessageId,
											sender: $"{update.Message.From!.FirstName}{update.Message.From!.LastName?.Let(lastName => " " + lastName)}",
											text: update.Message.Text!,
											imageBase64: null,
											replyToMessageId: update.Message.ReplyToMessage?.MessageId
										);
										await OpenAI.StreamChatWithFriendlyBotAsync(botClient, _serviceProvider,
											message: update.Message,
											thread: threadTracker.GetThread(
												messageId: replyToMessage.MessageId,
												maxLines: 20
											).ToImmutableList(),
											cancellationToken: cancellationToken
										);
									} else {
										await OpenAI.StreamChatWithFriendlyBotAsync(botClient, _serviceProvider,
											update.Message, cancellationToken);
									}
									break;
								case "Pakde":
									Message? sentMessage = await OpenAI.ChatWithSarcasticBotAsync(botClient,
										_serviceProvider, update.Message, callSign, cancellationToken);
									if (sentMessage is not null) {
										// Track sent message
										_serviceProvider.GetRequiredService<ThreadTracker>().TrackMessage(
											messageId: sentMessage.MessageId,
											sender: callSign,
											text: sentMessage.Text!,
											imageBase64: null,
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
							&& update.Message.Entities?.FirstOrDefault(entity =>
								entity is { Type: MessageEntityType.BotCommand, Offset: 0 }) is null
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
								imageBase64: null,
								replyToMessageId: replyToMessageId
							);

							// Get thread
							ImmutableList<(string Sender, string? Text, string? ImageBase64)> thread = threadTracker
								.GetThread(
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
										await OpenAI.StreamChatWithFriendlyBotAsync(botClient, _serviceProvider,
											update.Message, thread, cancellationToken);
										break;
									case "Pakde":
										Message? sentMessage = await OpenAI.ChatWithSarcasticBotAsync(botClient,
											_serviceProvider, update.Message, thread, callSign, cancellationToken);
										if (sentMessage is not null) {
											// Track sent message
											threadTracker.TrackMessage(
												messageId: sentMessage.MessageId,
												sender: callSign,
												text: sentMessage.Text!,
												imageBase64: null,
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
						if (update.Message?.Entities?.FirstOrDefault(entity =>
								entity is { Type: MessageEntityType.BotCommand, Offset: 0 }) is { } commandEntity) {
							string command = update.Message.Text!.Substring(commandEntity.Offset, commandEntity.Length);

							// Check if command is in /command@botname format
							int ampersandPos = command.IndexOf('@');
							if (ampersandPos != -1) {
								string targetUsername = command[(ampersandPos + 1)..];

								// Command is not for me
								if (!StringComparer.InvariantCultureIgnoreCase.Equals(targetUsername,
										(await GetMeAsync(botClient, cancellationToken)).Username)) break;

								// Normalize command
								command = command[..ampersandPos];
							}

							switch (command.ToLowerInvariant()) {
								case "/flip":
								case "/flop":
								case "/flap":
								case "/flep":
								case "/evaljs":
								case "/evalcs":
								case "/fuck":
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
								case "/sqlite3":
								case "/commonlisp":
								case "/cpp":
								case "/cs":
								case "/fs":
								case "/js":
								case "/ts":
								case "/vb":
								case "/pop":
								case "/ask":
								case "/humor":
								case "/primbon":
									if (SlashCommand.TryCreate(update.Message!, out SlashCommand? slashCommand)) {
										await _serviceProvider.GetRequiredService<ICommandQueue>().DispatchAsync(
											command: slashCommand
										);
									}
									break;
								case "/clean":
									await Clean.SanitizeLinkAsync(botClient, _serviceProvider, update.Message,
										cancellationToken);
									break;
								case "/art":
									await Art.GetRandomArtAsync(botClient, _serviceProvider, update.Message,
										cancellationToken);
									break;
								case "/tldr":
									await OpenAI.GenerateTldrAsync(botClient, _serviceProvider, update.Message,
										cancellationToken);
									break;
								case "/webp":
									await Webp.ConvertToImageAsync(botClient, update.Message, cancellationToken);
									break;
								case "/map":
									await SearchPlace.SearchPlaceAsync(botClient, _serviceProvider, update.Message,
										cancellationToken);
									break;
								case "/weather":
									await Weather.GetWeatherAsync(botClient, _serviceProvider, update.Message,
										cancellationToken);
									break;
								case "/bmkg":
									await BMKG.GetLatestEarthQuakeAsync(botClient, _serviceProvider, update.Message,
										cancellationToken);
									break;
								case "/preview":
									await Preview.GetPreviewAsync(botClient, _serviceProvider, update.Message,
										cancellationToken);
									break;
								case "/ramad":
									await Meme.HandleRamadAsync(botClient, _serviceProvider, update.Message,
										cancellationToken);
									break;
							}
						}

						break;
					case UpdateType.InlineQuery:
						if (update.InlineQuery?.Query.Trim().ToLowerInvariant() is { Length: > 0 } query) {
							IEnumerable<InlineQueryResult> inlineQueryResults =
								await _inlineQueryHandler.GetResultsAsync(query, cancellationToken);
							await botClient.AnswerInlineQueryAsync(
								inlineQueryId: update.InlineQuery.Id,
								results: inlineQueryResults,
								cancellationToken: cancellationToken);
						}

						break;
					case UpdateType.CallbackQuery:
						await _mediator.Send(
							new CallbackQueryUpdate(update.CallbackQuery!)
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

		public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
			CancellationToken cancellationToken) {
			string errorMessage = exception switch {
				ApiRequestException apiRequestException =>
					$"Telegram API Error:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
				_ => exception.ToString()
			};
			_logger.LogError(exception, "{message}", errorMessage);
			return Task.CompletedTask;
		}
	}
}
