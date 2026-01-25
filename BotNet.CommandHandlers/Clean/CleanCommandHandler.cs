using BotNet.Commands.Clean;
using Mediator;
using BotNet.Services.Instagram;
using BotNet.Services.Tiktok;
using BotNet.Services.Tokopedia;
using BotNet.Services.Twitter;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Clean {
	public sealed class CleanCommandHandler(
		ITelegramBotClient telegramBotClient,
		TiktokLinkSanitizer tiktokLinkSanitizer,
		TokopediaLinkSanitizer tokopediaLinkSanitizer
	) : ICommandHandler<CleanCommand> {
		public async ValueTask<Unit> Handle(CleanCommand command, CancellationToken cancellationToken) {
			string textToSanitize;
			int replyToMessageId;

			// Check if there's a command argument
			if (!string.IsNullOrWhiteSpace(command.Command.Text)) {
				textToSanitize = command.Command.Text;
				replyToMessageId = command.Command.MessageId;
			}
			// Otherwise check if replying to a message
			else if (command.Command.ReplyToMessage?.Text is { } repliedToMessage) {
				textToSanitize = repliedToMessage;
				replyToMessageId = command.Command.ReplyToMessage.MessageId;
			}
			else {
				await telegramBotClient.SendMessage(
					chatId: command.Command.Chat.Id,
					text: "<code>Tidak ada teks untuk dibersihkan. Balas pesan yang berisi link atau kirim link setelah perintah /clean.</code>",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters {
						MessageId = command.Command.MessageId
					},
					cancellationToken: cancellationToken
				);
				return default;
			}

			// Try to find and sanitize different types of links
			if (TiktokLinkSanitizer.FindShortenedTiktokLink(textToSanitize) is Uri shortenedTiktokUri) {
				try {
					Uri sanitizedLinkUri = await tiktokLinkSanitizer.SanitizeAsync(shortenedTiktokUri, cancellationToken);
					await telegramBotClient.SendMessage(
						chatId: command.Command.Chat.Id,
						text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
						replyParameters: new ReplyParameters {
							MessageId = replyToMessageId
						},
						cancellationToken: cancellationToken
					);
				} catch {
					await telegramBotClient.SendMessage(
						chatId: command.Command.Chat.Id,
						text: "<code>Link gagal dibersihkan.</code>",
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters {
							MessageId = command.Command.MessageId
						},
						cancellationToken: cancellationToken
					);
				}
			} else if (TwitterLinkSanitizer.FindTrackedTwitterLink(textToSanitize) is Uri trackedTwitterUri) {
				Uri sanitizedLinkUri = TwitterLinkSanitizer.Sanitize(trackedTwitterUri);
				await telegramBotClient.SendMessage(
					chatId: command.Command.Chat.Id,
					text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
					replyParameters: new ReplyParameters {
						MessageId = replyToMessageId
					},
					cancellationToken: cancellationToken
				);
			} else if (XLinkSanitizer.FindTrackedXLink(textToSanitize) is Uri trackedXUri) {
				Uri sanitizedLinkUri = XLinkSanitizer.Sanitize(trackedXUri);
				await telegramBotClient.SendMessage(
					chatId: command.Command.Chat.Id,
					text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
					replyParameters: new ReplyParameters {
						MessageId = replyToMessageId
					},
					cancellationToken: cancellationToken
				);
			} else if (InstagramLinkSanitizer.FindTrackedInstagramLink(textToSanitize) is Uri trackedInstagramUri) {
				Uri sanitizedLinkUri = InstagramLinkSanitizer.Sanitize(trackedInstagramUri);
				await telegramBotClient.SendMessage(
					chatId: command.Command.Chat.Id,
					text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
					replyParameters: new ReplyParameters {
						MessageId = replyToMessageId
					},
					cancellationToken: cancellationToken
				);
			} else if (TokopediaLinkSanitizer.FindShortenedLink(textToSanitize) is Uri trackerTokopediaUri) {
				try {
					Uri sanitizedLinkUri = await tokopediaLinkSanitizer.SanitizeAsync(trackerTokopediaUri, cancellationToken);
					await telegramBotClient.SendMessage(
						chatId: command.Command.Chat.Id,
						text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
						replyParameters: new ReplyParameters {
							MessageId = replyToMessageId
						},
						cancellationToken: cancellationToken
					);
				} catch {
					await telegramBotClient.SendMessage(
						chatId: command.Command.Chat.Id,
						text: "<code>Link gagal dibersihkan.</code>",
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters {
							MessageId = command.Command.MessageId
						},
						cancellationToken: cancellationToken
					);
				}
			} else {
				await telegramBotClient.SendMessage(
					chatId: command.Command.Chat.Id,
					text: "<code>Tidak ada link kotor yang dikenali.</code>",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters {
						MessageId = command.Command.MessageId
					},
					cancellationToken: cancellationToken
				);
			}

			return default;
		}
	}
}
