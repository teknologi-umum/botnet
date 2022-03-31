using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.ClearScript;
using BotNet.Services.DynamicExpresso;
using Microsoft.ClearScript;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Eval {
		private static readonly JsonSerializerOptions JSON_OPTIONS = new() { WriteIndented = true };

		public static async Task EvalJSAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					try {
						string result = await serviceProvider.GetRequiredService<V8Evaluator>().EvaluateAsync(commandArgument, cancellationToken);
						if (result.Length > 1000) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Result is too long.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: result.Length >= 2 && result[0] == '"' && result[^1] == '"'
									? $"Expression:\n<code>{WebUtility.HtmlEncode(commandArgument)}</code>\n\nString Result:\n<code>{WebUtility.HtmlEncode(result[1..^1].Replace("\\n", "\n"))}</code>"
									: $"Expression:\n<code>{WebUtility.HtmlEncode(commandArgument)}</code>\n\nResult:\n<code>{WebUtility.HtmlEncode(result)}</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						}
					} catch (ScriptEngineException exc) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>" + WebUtility.HtmlEncode(exc.Message) + "</code>",
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
					} catch (JsonException exc) when (exc.Message.Contains("A possible object cycle was detected.")) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>A possible object cycle was detected.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else if (message.ReplyToMessage?.Text is string repliedToMessage) {
					try {
						string result = await serviceProvider.GetRequiredService<V8Evaluator>().EvaluateAsync(repliedToMessage, cancellationToken);
						if (result.Length > 1000) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Result is too long.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: result.Length >= 2 && result[0] == '"' && result[^1] == '"'
									? $"Expression:\n<code>{WebUtility.HtmlEncode(repliedToMessage)}</code>\n\nString Result:\n<code>{WebUtility.HtmlEncode(result[1..^1].Replace("\\n", "\n"))}</code>"
									: $"Expression:\n<code>{WebUtility.HtmlEncode(repliedToMessage)}</code>\n\nResult:\n<code>{WebUtility.HtmlEncode(result)}</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.ReplyToMessage.MessageId,
								cancellationToken: cancellationToken);
						}
					} catch (ScriptEngineException exc) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>" + WebUtility.HtmlEncode(exc.Message) + "</code>",
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
					} catch (JsonException exc) when (exc.Message.Contains("A possible object cycle was detected.")) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>A possible object cycle was detected.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: "Untuk mengevaluasi javascript, silahkan ketik /evaljs diikuti ekspresi javascript.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
		}

		public static async Task EvalCSAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					try {
						object result = serviceProvider.GetRequiredService<CSharpEvaluator>().Evaluate(commandArgument);
						string prettifiedResult = JsonSerializer.Serialize(result, JSON_OPTIONS);
						if (prettifiedResult.Length > 1000) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Result is too long.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: prettifiedResult.Length >= 2 && prettifiedResult[0] == '"' && prettifiedResult[^1] == '"'
									? $"Expression:\n<code>{WebUtility.HtmlEncode(commandArgument)}</code>\n\nString Result:\n<code>{WebUtility.HtmlEncode(prettifiedResult[1..^1].Replace("\\n", "\n"))}</code>"
									: $"Expression:\n<code>{WebUtility.HtmlEncode(commandArgument)}</code>\n\nResult:\n<code>{WebUtility.HtmlEncode(prettifiedResult)}</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						}
					} catch (Exception exc) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>" + WebUtility.HtmlEncode(exc.Message) + "</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					}
				} else if (message.ReplyToMessage?.Text is string repliedToMessage) {
					try {
						object result = serviceProvider.GetRequiredService<CSharpEvaluator>().Evaluate(repliedToMessage);
						string prettifiedResult = JsonSerializer.Serialize(result, JSON_OPTIONS);
						if (prettifiedResult.Length > 1000) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: "<code>Result is too long.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: prettifiedResult.Length >= 2 && prettifiedResult[0] == '"' && prettifiedResult[^1] == '"'
									? $"Expression:\n<code>{WebUtility.HtmlEncode(repliedToMessage)}</code>\n\nString Result:\n<code>{WebUtility.HtmlEncode(prettifiedResult[1..^1].Replace("\\n", "\n"))}</code>"
									: $"Expression:\n<code>{WebUtility.HtmlEncode(repliedToMessage)}</code>\n\nResult:\n<code>{WebUtility.HtmlEncode(prettifiedResult)}</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.ReplyToMessage.MessageId,
								cancellationToken: cancellationToken);
						}
					} catch (Exception exc) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: "<code>" + WebUtility.HtmlEncode(exc.Message) + "</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
					}
				} else {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: "Untuk mengevaluasi C#, silahkan ketik /evalcs diikuti ekspresi C#.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
		}
	}
}
