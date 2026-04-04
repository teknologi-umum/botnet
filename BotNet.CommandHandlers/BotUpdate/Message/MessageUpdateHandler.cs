using BotNet.Commands;
using BotNet.Commands.BotUpdate.Message;
using Mediator;
using BotNet.Commands.CommandPrioritization;
using BotNet.Commands.SQL;
using BotNet.Services.BotProfile;
using BotNet.Services.SocialLink;
using BotNet.Services.SpamProtection;
using Microsoft.Extensions.Logging;
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
		CommandPriorityCategorizer commandPriorityCategorizer,
		SpamBanNotifier spamBanNotifier,
		ILogger<MessageUpdateHandler> logger
	) : ICommandHandler<MessageUpdate> {
		public async ValueTask<Unit> Handle(
			MessageUpdate update,
			CancellationToken cancellationToken
		) {
			// Ban new users posting netlify.app links
			if (update.Message.From is { Id: > 7000000000 } user &&
			    update.Message.Chat.Type is ChatType.Group or ChatType.Supergroup) {
				string messageText = (update.Message.Text ?? update.Message.Caption ?? string.Empty).ToLowerInvariant();
				
				if (messageText.Contains("netlify.app")) {
					try {
						await telegramBotClient.BanChatMember(
							chatId: update.Message.Chat.Id,
							userId: user.Id,
							cancellationToken: cancellationToken
						);
						
						string displayName = user.FirstName + (user.LastName != null ? $" {user.LastName}" : string.Empty);
						
						logger.LogInformation(
							"Banned user {UserId} ({UserName}) from chat {ChatId} for posting netlify.app link",
							user.Id,
							displayName,
							update.Message.Chat.Id
						);
						
						// Delete the spam message
						await telegramBotClient.DeleteMessage(
							chatId: update.Message.Chat.Id,
							messageId: update.Message.MessageId,
							cancellationToken: cancellationToken
						);
						
						// Send rate-limited notification
						await spamBanNotifier.NotifyBanAsync(
							chatId: update.Message.Chat.Id,
							displayName: displayName,
							cancellationToken: cancellationToken
						);
					} catch (Exception exc) {
						logger.LogError(
							exc,
							"Failed to ban user {UserId} from chat {ChatId}",
							user.Id,
							update.Message.Chat.Id
						);
					}
					
					return default;
				}
			}

			// Ban new users sharing non-Indonesian contacts in home group
			if (update.Message.Contact is { PhoneNumber: { } phoneNumber } &&
			    update.Message.From is { Id: > 7000000000 } contactUser &&
			    commandPriorityCategorizer.IsHomeGroup(update.Message.Chat.Id)) {
				string normalizedPhone = phoneNumber.TrimStart('+');
				if (!normalizedPhone.StartsWith("62")) {
					try {
						await telegramBotClient.BanChatMember(
							chatId: update.Message.Chat.Id,
							userId: contactUser.Id,
							cancellationToken: cancellationToken
						);

						string displayName = contactUser.FirstName + (contactUser.LastName != null ? $" {contactUser.LastName}" : string.Empty);

						logger.LogInformation(
							"Banned user {UserId} ({UserName}) from chat {ChatId} for sharing non-Indonesian contact",
							contactUser.Id,
							displayName,
							update.Message.Chat.Id
						);

						// Delete the contact message
						await telegramBotClient.DeleteMessage(
							chatId: update.Message.Chat.Id,
							messageId: update.Message.MessageId,
							cancellationToken: cancellationToken
						);

						// Send rate-limited notification
						await spamBanNotifier.NotifyBanAsync(
							chatId: update.Message.Chat.Id,
							displayName: displayName,
							cancellationToken: cancellationToken
						);
					} catch (Exception exc) {
						logger.LogError(
							exc,
							"Failed to ban user {UserId} from chat {ChatId}",
							contactUser.Id,
							update.Message.Chat.Id
						);
					}

					return default;
				}
			}

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

				return default;
			}

			// Handle Social Link (better preview)
			if ((update.Message.Text ?? update.Message.Caption) is { } textOrCaption) {
				List<Uri> possibleUrls = SocialLinkEmbedFixer.GetPossibleUrls(textOrCaption)
					.ToList();

				if (possibleUrls.Any()) {
					// Fire and forget
					BackgroundTask.Run(async () => {
						foreach (Uri url in possibleUrls) {
							Uri fixedUrl = SocialLinkEmbedFixer.Fix(url);
							await telegramBotClient.SendMessage(
								chatId: update.Message.Chat.Id,
								text: $"Preview: {fixedUrl.OriginalString}",
								replyParameters: new ReplyParameters { MessageId = update.Message.MessageId },
								cancellationToken: cancellationToken
							);
						}
					}, logger);
					return default;
				}
			}

			// Handle reddit mirroring
		if (update.Message.Entities?.FirstOrDefault(
			    entity => entity is {
				    Type: MessageEntityType.Url
			    }
		    ) is {
			    Offset: int offset,
			    Length: int length
		    } &&
		    update.Message.Text?.Substring(offset, length) is { } url &&
		    url.StartsWith("https://www.reddit.com/", out string? remainingUrl)) {
				// Fire and forget
				BackgroundTask.Run(async () => {
					await telegramBotClient.SendMessage(
						chatId: update.Message.Chat.Id,
						text: $"Mirror: https://libreddit.teknologiumum.com/{remainingUrl}",
						replyParameters: new ReplyParameters { MessageId = update.Message.MessageId },
						linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
						cancellationToken: cancellationToken
					);
				}, logger);
				return default;
			}

			if (update.Message.Entities?.FirstOrDefault(
				    entity => entity is {
					    Type: MessageEntityType.TextLink
				    }
			    ) is { Url: { } textUrl } &&
			    textUrl.StartsWith("https://www.reddit.com/", out string? remainingTextUrl)) {
				// Fire and forget
				BackgroundTask.Run(async () => {
					await telegramBotClient.SendMessage(
						chatId: update.Message.Chat.Id,
						text: $"Mirror: https://libreddit.teknologiumum.com/{remainingTextUrl}",
						replyParameters: new ReplyParameters { MessageId = update.Message.MessageId },
						linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
						cancellationToken: cancellationToken
					);
				}, logger);
				return default;
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
				return default;
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
					return default;
				}

				// Cache follow up message
				telegramMessageCache.Add(
					message: aiFollowUpMessage
				);

				await commandQueue.DispatchAsync(aiFollowUpMessage);
				return default;
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
						BackgroundTask.Run(async () => {
							await telegramBotClient.SendMessage(
								chatId: update.Message.Chat.Id,
								text: $"<code>Your SQL contains more than one statement.</code>",
								parseMode: ParseMode.Html,
								replyParameters: new ReplyParameters { MessageId = update.Message.MessageId },
								cancellationToken: cancellationToken
							);
						}, logger);
						return default;
					}

					if (ast[0] is not Statement.Select) {
						// Fire and forget
						BackgroundTask.Run(async () => {
							await telegramBotClient.SendMessage(
								chatId: update.Message.Chat.Id,
								text: $"<code>Your SQL is not a SELECT statement.</code>",
								parseMode: ParseMode.Html,
								replyParameters: new ReplyParameters { MessageId = update.Message.MessageId },
								cancellationToken: cancellationToken
							);
						}, logger);
						return default;
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
			return default;
		}
	}
}
