using BotNet.Commands;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.CommandPrioritization;
using BotNet.Services.BotProfile;
using BotNet.Services.SocialLink;
using RG.Ninja;
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

			// Handle Social Link (better preview)
			if ((update.Message.Text ?? update.Message.Caption) is { } textOrCaption) {
				IEnumerable<Uri> possibleUrls = SocialLinkEmbedFixer.GetPossibleUrls(textOrCaption);

				if (possibleUrls.Any()) {
					// Fire and forget
					Task _ = Task.Run(async () => {
						try {
							foreach (Uri url in possibleUrls) {
								Uri fixedUrl = SocialLinkEmbedFixer.Fix(url);
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
			}

			// Handle reddit mirroring
			if (update.Message?.Entities?.FirstOrDefault(entity => entity is {
				Type: MessageEntityType.Url
			}) is {
				Offset: var offset,
				Length: var length
			} && update.Message.Text?.Substring(offset, length) is { } url
			&& url.StartsWith("https://www.reddit.com/", out string? remainingUrl)) {
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
			&& textUrl.StartsWith("https://www.reddit.com/", out string? remainingTextUrl)) {
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
			}
		}
	}
}
