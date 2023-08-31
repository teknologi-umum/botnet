using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Instagram;
using BotNet.Services.Tiktok;
using BotNet.Services.Tokopedia;
using BotNet.Services.Twitter;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Clean {
		public static async Task SanitizeLinkAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					if (TiktokLinkSanitizer.FindShortenedTiktokLink(commandArgument) is Uri shortenedTiktokUri) {
						try {
							Uri sanitizedLinkUri = await serviceProvider.GetRequiredService<TiktokLinkSanitizer>().SanitizeAsync(shortenedTiktokUri, cancellationToken);
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} catch {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Link gagal dibersihkan.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						}
					} else if (TwitterLinkSanitizer.FindTrackedTwitterLink(commandArgument) is Uri trackedTwitterUri) {
						Uri sanitizedLinkUri = TwitterLinkSanitizer.Sanitize(trackedTwitterUri);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} else if (XLinkSanitizer.FindTrackedXLink(commandArgument) is Uri trackedXUri) {
						Uri sanitizedLinkUri = XLinkSanitizer.Sanitize(trackedXUri);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} else if (InstagramLinkSanitizer.FindTrackedInstagramLink(commandArgument) is Uri trackedInstagramUri) {
						Uri sanitizedLinkUri = InstagramLinkSanitizer.Sanitize(trackedInstagramUri);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} else if (TokopediaLinkSanitizer.FindShortenedLink(commandArgument) is Uri trackerTokopediaUri) {
						try {
							Uri sanitizedLinkUri = await serviceProvider.GetRequiredService<TokopediaLinkSanitizer>().SanitizeAsync(trackerTokopediaUri, cancellationToken);
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} catch {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Link gagal dibersihkan.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						}
					} else {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>Tidak ada link kotor yang dikenali.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else if (message.ReplyToMessage?.Text is string repliedToMessage) {
					if (TiktokLinkSanitizer.FindShortenedTiktokLink(repliedToMessage) is Uri shortenedTiktokUri) {
						try {
							Uri sanitizedLinkUri = await serviceProvider.GetRequiredService<TiktokLinkSanitizer>().SanitizeAsync(shortenedTiktokUri, cancellationToken);
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
								replyToMessageId: message.ReplyToMessage.MessageId,
								cancellationToken: cancellationToken);
						} catch {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Link gagal dibersihkan.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						}
					} else if (TwitterLinkSanitizer.FindTrackedTwitterLink(repliedToMessage) is Uri trackedTwitterUri) {
						Uri sanitizedLinkUri = TwitterLinkSanitizer.Sanitize(trackedTwitterUri);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
					} else if (XLinkSanitizer.FindTrackedXLink(repliedToMessage) is Uri trackedXUri) {
						Uri sanitizedLinkUri = XLinkSanitizer.Sanitize(trackedXUri);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
					} else if (InstagramLinkSanitizer.FindTrackedInstagramLink(repliedToMessage) is Uri trackedInstagramUri) {
						Uri sanitizedLinkUri = InstagramLinkSanitizer.Sanitize(trackedInstagramUri);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
					} else if (TokopediaLinkSanitizer.FindShortenedLink(repliedToMessage) is Uri trackerTokopediaUri) {
						try {
							Uri sanitizedLinkUri = await serviceProvider.GetRequiredService<TokopediaLinkSanitizer>().SanitizeAsync(trackerTokopediaUri, cancellationToken);
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"Link yang sudah dibersihkan: {sanitizedLinkUri.OriginalString}",
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} catch {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Link gagal dibersihkan.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						}
					} else {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>Tidak ada link kotor yang dikenali.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				}
			}
		}
	}
}
