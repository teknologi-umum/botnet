using BotNet.Commands.Clean;
using Mediator;
using BotNet.Services.Instagram;
using BotNet.Services.Tiktok;
using BotNet.Services.Tokopedia;
using BotNet.Services.Twitter;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Clean {
	public sealed class CleanCommandHandler(
		ITelegramBotClient telegramBotClient,
		TiktokLinkSanitizer tiktokLinkSanitizer,
		TokopediaLinkSanitizer tokopediaLinkSanitizer,
		ILogger<CleanCommandHandler> logger
	) : ICommandHandler<CleanCommand> {
		public async ValueTask<Unit> Handle(CleanCommand command, CancellationToken cancellationToken) {
			// Determine which text to sanitize and which message to reply to
			string textToSanitize = command.TextToClean ?? command.ReplyToMessageText!;
			int replyToMessageId = command.ReplyToMessageId?.Value ?? command.CommandMessageId.Value;

			// Try to find and sanitize different types of links
			if (TiktokLinkSanitizer.FindShortenedTiktokLink(textToSanitize) is Uri shortenedTiktokUri) {
				await SanitizeLinkAsync(
					async () => await tiktokLinkSanitizer.SanitizeAsync(shortenedTiktokUri, cancellationToken),
					command.Chat.Id,
					replyToMessageId,
					command.CommandMessageId.Value,
					"TikTok",
					cancellationToken
				);
			} else if (TwitterLinkSanitizer.FindTrackedTwitterLink(textToSanitize) is Uri trackedTwitterUri) {
				Uri sanitizedLinkUri = TwitterLinkSanitizer.Sanitize(trackedTwitterUri);
				await SendCleanedLinkAsync(
					sanitizedLinkUri,
					command.Chat.Id,
					replyToMessageId,
					cancellationToken
				);
			} else if (XLinkSanitizer.FindTrackedXLink(textToSanitize) is Uri trackedXUri) {
				Uri sanitizedLinkUri = XLinkSanitizer.Sanitize(trackedXUri);
				await SendCleanedLinkAsync(
					sanitizedLinkUri,
					command.Chat.Id,
					replyToMessageId,
					cancellationToken
				);
			} else if (InstagramLinkSanitizer.FindTrackedInstagramLink(textToSanitize) is Uri trackedInstagramUri) {
				Uri sanitizedLinkUri = InstagramLinkSanitizer.Sanitize(trackedInstagramUri);
				await SendCleanedLinkAsync(
					sanitizedLinkUri,
					command.Chat.Id,
					replyToMessageId,
					cancellationToken
				);
			} else if (TokopediaLinkSanitizer.FindShortenedLink(textToSanitize) is Uri trackerTokopediaUri) {
				await SanitizeLinkAsync(
					async () => await tokopediaLinkSanitizer.SanitizeAsync(trackerTokopediaUri, cancellationToken),
					command.Chat.Id,
					replyToMessageId,
					command.CommandMessageId.Value,
					"Tokopedia",
					cancellationToken
				);
			} else {
				await SendMessageAsync(
					command.Chat.Id,
					"<code>Tidak ada link kotor yang dikenali.</code>",
					ParseMode.Html,
					command.CommandMessageId.Value,
					cancellationToken
				);
			}

			return default;
		}

		private async Task SanitizeLinkAsync(
			Func<Task<Uri>> sanitizeFunc,
			long chatId,
			int replyToMessageId,
			int errorReplyToMessageId,
			string linkType,
			CancellationToken cancellationToken
		) {
			try {
				Uri sanitizedLinkUri = await sanitizeFunc();
				await SendCleanedLinkAsync(
					sanitizedLinkUri,
					chatId,
					replyToMessageId,
					cancellationToken
				);
			} catch (Exception exc) {
				logger.LogError(exc, "Failed to sanitize {LinkType} link", linkType);
				await SendMessageAsync(
					chatId,
					"<code>Link gagal dibersihkan.</code>",
					ParseMode.Html,
					errorReplyToMessageId,
					cancellationToken
				);
			}
		}

		private async Task SendCleanedLinkAsync(
			Uri sanitizedLinkUri,
			long chatId,
			int replyToMessageId,
			CancellationToken cancellationToken
		) {
			await SendMessageAsync(
				chatId,
				$"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
				null,
				replyToMessageId,
				cancellationToken
			);
		}

		private async Task SendMessageAsync(
			long chatId,
			string text,
			ParseMode? parseMode,
			int replyToMessageId,
			CancellationToken cancellationToken
		) {
			if (parseMode.HasValue) {
				await telegramBotClient.SendMessage(
					chatId: chatId,
					text: text,
					parseMode: parseMode.Value,
					replyParameters: new ReplyParameters {
						MessageId = replyToMessageId
					},
					cancellationToken: cancellationToken
				);
			} else {
				await telegramBotClient.SendMessage(
					chatId: chatId,
					text: text,
					replyParameters: new ReplyParameters {
						MessageId = replyToMessageId
					},
					cancellationToken: cancellationToken
				);
			}
		}
	}
}
