using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.MarkdownV2;
using BotNet.Services.Piston;
using BotNet.Services.Piston.Models;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Exec {
		private static DateTimeOffset _skipPestoUntil = DateTimeOffset.Now;

		public static async Task ExecAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, string language, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					// Use Piston
					try {
						ExecuteResult result = await serviceProvider.GetRequiredService<PistonClient>()
							.ExecuteAsync(language.ToLowerInvariant(), commandArgument, cancellationToken);

						if (result.Compile is { Code: not 0 }) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>{WebUtility.HtmlEncode(result.Compile.Stderr)}</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else if (result.Run.Code != 0) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>{WebUtility.HtmlEncode(result.Run.Stderr)}</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else if (result.Run.Output.Length > 1000) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Output is too long.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"Code:\n```{language}\n{MarkdownV2Sanitizer.Sanitize(commandArgument)}\n```\n\nOutput:\n```\n{MarkdownV2Sanitizer.Sanitize(result.Run.Output)}\n```",
								parseMode: ParseMode.MarkdownV2,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						}
#pragma warning disable CS0618 // Type or member is obsolete
					} catch (ExecutionEngineException exc) {
#pragma warning restore CS0618 // Type or member is obsolete
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>" + WebUtility.HtmlEncode(exc.Message ?? "Unknown error") + "</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} catch (OperationCanceledException) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>Timeout exceeded.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else if (message.ReplyToMessage?.Text is string repliedToMessage) {
					try {
						ExecuteResult result = await serviceProvider.GetRequiredService<PistonClient>()
							.ExecuteAsync(language.ToLowerInvariant(), repliedToMessage, cancellationToken);

						if (result.Compile is { Code: not 0 }) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>{WebUtility.HtmlEncode(result.Compile.Stderr)}</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else if (result.Run.Code != 0) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>{WebUtility.HtmlEncode(result.Run.Stderr)}</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else if (result.Run.Output.Length > 1000) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Output is too long.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"Code:\n```{language}\n{MarkdownV2Sanitizer.Sanitize(repliedToMessage)}\n```\n\nOutput:\n```\n{MarkdownV2Sanitizer.Sanitize(result.Run.Output)}\n```",
								parseMode: ParseMode.MarkdownV2,
								replyToMessageId: message.ReplyToMessage.MessageId,
								cancellationToken: cancellationToken);
						}
#pragma warning disable CS0618 // Type or member is obsolete
					} catch (ExecutionEngineException exc) {
#pragma warning restore CS0618 // Type or member is obsolete
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>" + WebUtility.HtmlEncode(exc.Message ?? "Unknown error") + "</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
					} catch (OperationCanceledException) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>Timeout exceeded.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: $"Untuk mengeksekusi program, silakan ketik {message.Text![..commandLength].Trim()} diikuti code.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
		}
	}
}
