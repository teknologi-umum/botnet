using System.Text.RegularExpressions;
using BotNet.Commands;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.CommandPrioritization;
using BotNet.Commands.SQL;
using BotNet.Services.BotProfile;
using BotNet.Services.SocialLink;
using BotNet.Services.UrlCleaner;
using RG.Ninja;
using SqlParser;
using SqlParser.Ast;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.BotUpdate.Message {
	public sealed class MessageUpdateHandler(
		ITelegramBotClient telegramBotClient,
		ICommandQueue commandQueue,
		ITelegramMessageCache telegramMessageCache,
		BotProfileAccessor botProfileAccessor,
		CommandPriorityCategorizer commandPriorityCategorizer
	) : ICommandHandler<MessageUpdate> {
		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly ICommandQueue _commandQueue = commandQueue;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;
		private readonly BotProfileAccessor _botProfileAccessor = botProfileAccessor;
		private readonly CommandPriorityCategorizer _commandPriorityCategorizer = commandPriorityCategorizer;

		public async Task Handle(MessageUpdate update, CancellationToken cancellationToken) {
			// Handle slash commands
			if (update.Message.Entities?.FirstOrDefault() is {
				Type: MessageEntityType.BotCommand,
				Offset: 0
			}) {
				if (SlashCommand.TryCreate(
					message: update.Message,
					botUsername: (await _botProfileAccessor.GetBotProfileAsync(cancellationToken)).Username!,
					commandPriorityCategorizer: _commandPriorityCategorizer,
					out SlashCommand? slashCommand
				)) {
					await _commandQueue.DispatchAsync(
						command: slashCommand
					);
				}
				return;
			}

			if ((update.Message.Text ?? update.Message.Caption) is { } textOrCaption) {

				// Handle Social Link (better preview)
				IEnumerable<Uri> possibleSocialUrls = SocialLinkEmbedFixer.GetPossibleUrls(textOrCaption);
				if (possibleSocialUrls.Any()) {
					// Fire and forget
					Task _ = Task.Run(async () => {
						try {
							foreach (Uri url in possibleSocialUrls) {
								Uri cleanedUrl = UrlCleaner.Clean(url);
								Uri fixedUrl = SocialLinkEmbedFixer.Fix(cleanedUrl);
								await _telegramBotClient.SendTextMessageAsync(
									chatId: update.Message.Chat.Id,
									text: $"Preview: {fixedUrl.OriginalString}",
									replyToMessageId: update.Message.MessageId,
									cancellationToken: cancellationToken
								);
							}
						} catch (OperationCanceledException) {
							// Terminate gracefully
						}
					});
					return;
				}
				
				// get list of urls from message (start with http or https or www)
				string pattern = @"(?i)\b((?:https?://|www\.)\S+)\b";
				MatchCollection matches = Regex.Matches(textOrCaption, pattern);
				List<string> urls = matches.Select(m => m.Value).ToList();

				// Clean the url
				if (urls.Count > 0) {
					// Fire and forget
					Task _ = Task.Run(async () => {
						try {
							foreach (string url in urls) {
								Uri cleanedUrl = UrlCleaner.Clean(new Uri(url));

								// if the url is same, don't send the message
								if (cleanedUrl.OriginalString == new Uri(url).OriginalString) {
									continue;
								}

								await _telegramBotClient.SendTextMessageAsync(
									chatId: update.Message.Chat.Id,
									text: $"Cleaned URL: {cleanedUrl.OriginalString}",
									replyToMessageId: update.Message.MessageId,
									cancellationToken: cancellationToken
								);
							}
						} catch (OperationCanceledException) {
							// Terminate gracefully
						}
					});
					return;
				}

			}

			// Handle reddit mirroring
			if (update.Message?.Entities?.FirstOrDefault(entity => entity is {
				Type: MessageEntityType.Url
			}) is {
				Offset: var offset,
				Length: var length
			} && update.Message.Text?.Substring(offset, length) is { } url
			&& UrlCleaner.Clean(new Uri(url)) is { } cleanedUrl
			&& cleanedUrl.ToString().StartsWith("https://www.reddit.com/", out string? remainingUrl)) {
				// Fire and forget
				Task _ = Task.Run(async () => {
					try {
						await _telegramBotClient.SendTextMessageAsync(
							chatId: update.Message.Chat.Id,
							text: $"Mirror: https://libreddit.teknologiumum.com/{remainingUrl}",
							replyToMessageId: update.Message.MessageId,
							disableWebPagePreview: true,
							cancellationToken: cancellationToken
						);
					} catch (OperationCanceledException) {
						// Terminate gracefully
					}
				});
				return;
			} else if (update.Message?.Entities?.FirstOrDefault(entity => entity is {
				Type: MessageEntityType.TextLink
			}) is { Url: { } textUrl }
			&& UrlCleaner.Clean(new Uri(textUrl)) is { } cleanedTextUrl
			&& cleanedTextUrl.ToString().StartsWith("https://www.reddit.com/", out string? remainingTextUrl)) {
				// Fire and forget
				Task _ = Task.Run(async () => {
					try {
						await _telegramBotClient.SendTextMessageAsync(
							chatId: update.Message.Chat.Id,
							text: $"Mirror: https://libreddit.teknologiumum.com/{remainingTextUrl}",
							replyToMessageId: update.Message.MessageId,
							disableWebPagePreview: true,
							cancellationToken: cancellationToken
						);
					} catch (OperationCanceledException) {
						// Terminate gracefully
					}
				});
				return;
			}

			// Handle AI calls
			if (AICallCommand.TryCreate(
				message: update.Message!,
				commandPriorityCategorizer: _commandPriorityCategorizer,
				out AICallCommand? aiCallCommand
			)) {
				// Cache both message and reply to message
				_telegramMessageCache.Add(
					message: aiCallCommand
				);
				if (aiCallCommand.ReplyToMessage is { } replyToMessage) {
					_telegramMessageCache.Add(
						message: replyToMessage
					);
				}

				await _commandQueue.DispatchAsync(
					command: aiCallCommand
				);
				return;
			}

			// Handle AI follow up message
			if (update.Message is {
				ReplyToMessage.MessageId: int replyToMessageId,
				Chat.Id: long chatId
			} && _telegramMessageCache.GetOrDefault(
				messageId: new(replyToMessageId),
				chatId: new(chatId)
			) is AIResponseMessage) {
				if (!AIFollowUpMessage.TryCreate(
					message: update.Message,
					thread: _telegramMessageCache.GetThread(
						messageId: new(replyToMessageId),
						chatId: new(chatId)
					),
					commandPriorityCategorizer: _commandPriorityCategorizer,
					out AIFollowUpMessage? aiFollowUpMessage
				)) {
					return;
				}

				// Cache follow up message
				_telegramMessageCache.Add(
					message: aiFollowUpMessage
				);

				await _commandQueue.DispatchAsync(aiFollowUpMessage);
				return;
			}
			
			// Handle SQL
			if (update.Message is {
				ReplyToMessage: null,
				Text: { } text
			} && text.StartsWith("select", StringComparison.OrdinalIgnoreCase)) {
				try {
					Sequence<Statement> ast = new SqlParser.Parser().ParseSql(text);
					if (ast.Count > 1) {
						// Fire and forget
						Task _ = Task.Run(async () => {
							try {
								await _telegramBotClient.SendTextMessageAsync(
									chatId: update.Message.Chat.Id,
									text: $"<code>Your SQL contains more than one statement.</code>",
									parseMode: ParseMode.Html,
									replyToMessageId: update.Message.MessageId,
									cancellationToken: cancellationToken
								);
							} catch (OperationCanceledException) {
								// Terminate gracefully
							}
						});
						return;
					}
					if (ast[0] is not Statement.Select selectStatement) {
						// Fire and forget
						Task _ = Task.Run(async () => {
							try {
								await _telegramBotClient.SendTextMessageAsync(
									chatId: update.Message.Chat.Id,
									text: $"<code>Your SQL is not a SELECT statement.</code>",
									parseMode: ParseMode.Html,
									replyToMessageId: update.Message.MessageId,
									cancellationToken: cancellationToken
								);
							} catch (OperationCanceledException) {
								// Terminate gracefully
							}
						});
						return;
					}
					if (SQLCommand.TryCreate(
						message: update.Message,
						commandPriorityCategorizer: _commandPriorityCategorizer,
						sqlCommand: out SQLCommand? sqlCommand
					)) {
						await _commandQueue.DispatchAsync(
							command: sqlCommand
						);
						return;
					}
				} catch (ParserException exc) {
					await _telegramBotClient.SendTextMessageAsync(
						chatId: update.Message.Chat.Id,
						text: $"<code>{exc.Message}</code>",
						parseMode: ParseMode.Html,
						replyToMessageId: update.Message.MessageId,
						cancellationToken: cancellationToken
					);
				} catch {
					// Suppress
				}
			}
		}
	}
}
