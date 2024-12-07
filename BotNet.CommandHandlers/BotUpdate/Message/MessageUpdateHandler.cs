using BotNet.Commands;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.CommandPrioritization;
using BotNet.Commands.SQL;
using BotNet.Services.BotProfile;
using BotNet.Services.SocialLink;
using RG.Ninja;
using SqlParser;
using SqlParser.Ast;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.BotUpdate.Message {
	public sealed class MessageUpdateHandler(
		ITelegramBotClient telegramBotClient,
		ICommandQueue commandQueue,
		ITelegramMessageCache telegramMessageCache,
		BotProfileAccessor botProfileAccessor,
		CommandPriorityCategorizer commandPriorityCategorizer
	) : ICommandHandler<MessageUpdate> {
		public async Task Handle(
			MessageUpdate update,
			CancellationToken cancellationToken
		) {
			// Handle slash commands
			if (update.Message.Entities?.FirstOrDefault() is {
				    Type: MessageEntityType.BotCommand,
				    Offset: 0
			    }) {
				if (SlashCommand.TryCreate(
					    message: update.Message,
					    botUsername: (await botProfileAccessor.GetBotProfileAsync(cancellationToken)).Username!,
					    commandPriorityCategorizer: commandPriorityCategorizer,
					    out SlashCommand? slashCommand
				    )) {
					await commandQueue.DispatchAsync(
						command: slashCommand
					);
				}

				return;
			}

			// Handle Social Link (better preview)
			if ((update.Message.Text ?? update.Message.Caption) is { } textOrCaption) {
				List<Uri> possibleUrls = SocialLinkEmbedFixer.GetPossibleUrls(textOrCaption)
					.ToList();

				if (possibleUrls.Any()) {
					// Fire and forget
					Task _ = Task.Run(
						async () => {
							try {
								foreach (Uri url in possibleUrls) {
									Uri fixedUrl = SocialLinkEmbedFixer.Fix(url);
									await telegramBotClient.SendMessage(
										chatId: update.Message.Chat.Id,
										text: $"Preview: {fixedUrl.OriginalString}",
										replyParameters: new ReplyParameters { MessageId = update.Message.MessageId },
										cancellationToken: cancellationToken
									);
								}
							} catch (OperationCanceledException) {
								// Terminate gracefully
							}
						}
					);
					return;
				}
			}

			// Handle reddit mirroring
			if (update.Message.Entities?.FirstOrDefault(
				    entity => entity is {
					    Type: MessageEntityType.Url
				    }
			    ) is {
				    Offset: var offset,
				    Length: var length
			    } &&
			    update.Message.Text?.Substring(offset, length) is { } url &&
			    url.StartsWith("https://www.reddit.com/", out string? remainingUrl)) {
				// Fire and forget
				Task _ = Task.Run(
					async () => {
						try {
							await telegramBotClient.SendMessage(
								chatId: update.Message.Chat.Id,
								text: $"Mirror: https://libreddit.teknologiumum.com/{remainingUrl}",
								replyParameters: new ReplyParameters { MessageId = update.Message.MessageId },
								linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
								cancellationToken: cancellationToken
							);
						} catch (OperationCanceledException) {
							// Terminate gracefully
						}
					}
				);
				return;
			}

			if (update.Message.Entities?.FirstOrDefault(
				    entity => entity is {
					    Type: MessageEntityType.TextLink
				    }
			    ) is { Url: { } textUrl } &&
			    textUrl.StartsWith("https://www.reddit.com/", out string? remainingTextUrl)) {
				// Fire and forget
				Task _ = Task.Run(
					async () => {
						try {
							await telegramBotClient.SendMessage(
								chatId: update.Message.Chat.Id,
								text: $"Mirror: https://libreddit.teknologiumum.com/{remainingTextUrl}",
								replyParameters: new ReplyParameters { MessageId = update.Message.MessageId },
								linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
								cancellationToken: cancellationToken
							);
						} catch (OperationCanceledException) {
							// Terminate gracefully
						}
					}
				);
				return;
			}

			// Handle AI calls
			if (AiCallCommand.TryCreate(
				    message: update.Message,
				    commandPriorityCategorizer: commandPriorityCategorizer,
				    out AiCallCommand? aiCallCommand
			    )) {
				// Cache both message and reply to message
				telegramMessageCache.Add(
					message: aiCallCommand
				);
				if (aiCallCommand.ReplyToMessage is { } replyToMessage) {
					telegramMessageCache.Add(
						message: replyToMessage
					);
				}

				await commandQueue.DispatchAsync(
					command: aiCallCommand
				);
				return;
			}

			// Handle AI follow up message
			if (update.Message is {
				    ReplyToMessage.MessageId: int replyToMessageId,
				    Chat.Id: long chatId
			    } &&
			    telegramMessageCache.GetOrDefault(
				    messageId: new(replyToMessageId),
				    chatId: new(chatId)
			    ) is AiResponseMessage) {
				if (!AiFollowUpMessage.TryCreate(
					    message: update.Message,
					    thread: telegramMessageCache.GetThread(
						    messageId: new(replyToMessageId),
						    chatId: new(chatId)
					    ),
					    commandPriorityCategorizer: commandPriorityCategorizer,
					    out AiFollowUpMessage? aiFollowUpMessage
				    )) {
					return;
				}

				// Cache follow up message
				telegramMessageCache.Add(
					message: aiFollowUpMessage
				);

				await commandQueue.DispatchAsync(aiFollowUpMessage);
				return;
			}

			// Handle SQL
			if (update.Message is {
				    ReplyToMessage: null,
				    Text: { } text
			    } &&
			    text.StartsWith("select", StringComparison.OrdinalIgnoreCase)) {
				try {
					Sequence<Statement> ast = new Parser().ParseSql(text);
					if (ast.Count > 1) {
						// Fire and forget
						Task _ = Task.Run(
							async () => {
								try {
									await telegramBotClient.SendMessage(
										chatId: update.Message.Chat.Id,
										text: $"<code>Your SQL contains more than one statement.</code>",
										parseMode: ParseMode.Html,
										replyParameters: new ReplyParameters { MessageId = update.Message.MessageId },
										cancellationToken: cancellationToken
									);
								} catch (OperationCanceledException) {
									// Terminate gracefully
								}
							}
						);
						return;
					}

					if (ast[0] is not Statement.Select) {
						// Fire and forget
						Task _ = Task.Run(
							async () => {
								try {
									await telegramBotClient.SendMessage(
										chatId: update.Message.Chat.Id,
										text: $"<code>Your SQL is not a SELECT statement.</code>",
										parseMode: ParseMode.Html,
										replyParameters: new ReplyParameters { MessageId = update.Message.MessageId },
										cancellationToken: cancellationToken
									);
								} catch (OperationCanceledException) {
									// Terminate gracefully
								}
							}
						);
						return;
					}

					if (SqlCommand.TryCreate(
						    message: update.Message,
						    commandPriorityCategorizer: commandPriorityCategorizer,
						    sqlCommand: out SqlCommand? sqlCommand
					    )) {
						await commandQueue.DispatchAsync(
							command: sqlCommand
						);
					}
				} catch (ParserException exc) {
					await telegramBotClient.SendMessage(
						chatId: update.Message.Chat.Id,
						text: $"<code>{exc.Message}</code>",
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters { MessageId = update.Message.MessageId },
						cancellationToken: cancellationToken
					);
				} catch {
					// Suppress
				}
			}
		}
	}
}
