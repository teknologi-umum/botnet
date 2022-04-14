using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.OpenAI;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.DependencyInjection;
using RG.Ninja;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotNet.Services.BotCommands {
	public static class OpenAI {
		private static readonly RateLimiter EXPLAIN_GROUP_RATE_LIMITER = RateLimiter.PerUserPerChat(3, TimeSpan.FromMinutes(5));
		private static readonly RateLimiter EXPLAIN_PRIVATE_RATE_LIMITER = RateLimiter.PerUser(10, TimeSpan.FromMinutes(5));
		public static async Task ExplainAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, string language, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					try {
						(message.Chat.Type == ChatType.Private
							? EXPLAIN_PRIVATE_RATE_LIMITER
							: EXPLAIN_GROUP_RATE_LIMITER
						).ValidateActionRate(message.Chat.Id, message.From!.Id);
						string result = language switch {
							"en" => await serviceProvider.GetRequiredService<CodeExplainer>().ExplainCodeInEnglishAsync(commandArgument, cancellationToken),
							"id" => await serviceProvider.GetRequiredService<CodeExplainer>().ExplainCodeInIndonesianAsync(commandArgument, cancellationToken),
							_ => throw new NotImplementedException()
						};
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>{WebUtility.HtmlEncode(result)}</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
						if (message.Chat.Type == ChatType.Private) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>Anda terlalu banyak menggunakan /explain. Coba lagi {cooldown}.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>Anda terlalu banyak menggunakan /explain di sini. Coba lagi {cooldown} atau lanjutkan di private chat.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								replyMarkup: new InlineKeyboardMarkup(
									InlineKeyboardButton.WithUrl("Private chat 💬", "t.me/TeknumBot")
								),
								cancellationToken: cancellationToken);
						}
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
						(message.Chat.Type == ChatType.Private
							? EXPLAIN_PRIVATE_RATE_LIMITER
							: EXPLAIN_GROUP_RATE_LIMITER
						).ValidateActionRate(message.Chat.Id, message.From!.Id);
						string result = language switch {
							"en" => await serviceProvider.GetRequiredService<CodeExplainer>().ExplainCodeInEnglishAsync(repliedToMessage, cancellationToken),
							"id" => await serviceProvider.GetRequiredService<CodeExplainer>().ExplainCodeInIndonesianAsync(repliedToMessage, cancellationToken),
							_ => throw new NotImplementedException()
						};
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>{WebUtility.HtmlEncode(result)}</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
					} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
						if (message.Chat.Type == ChatType.Private) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>Anda terlalu banyak menggunakan /explain. Coba lagi {cooldown}.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>Anda terlalu banyak menggunakan /explain di sini. Coba lagi {cooldown} atau lanjutkan di private chat.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								replyMarkup: new InlineKeyboardMarkup(
									InlineKeyboardButton.WithUrl("Private chat 💬", "t.me/TeknumBot")
								),
								cancellationToken: cancellationToken);
						}
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
						text: "Untuk explain code, silahkan ketik /explain diikuti code.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
		}

		private static readonly RateLimiter ASK_GROUP_RATE_LIMITER = RateLimiter.PerUserPerChat(5, TimeSpan.FromMinutes(15));
		private static readonly RateLimiter ASK_PRIVATE_RATE_LIMITER = RateLimiter.PerUser(20, TimeSpan.FromMinutes(15));
		public static async Task AskHelpAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					try {
						(message.Chat.Type == ChatType.Private
							? ASK_PRIVATE_RATE_LIMITER
							: ASK_GROUP_RATE_LIMITER
						).ValidateActionRate(message.Chat.Id, message.From!.Id);
						string result = await serviceProvider.GetRequiredService<AssistantBot>().AskSomethingAsync(
							name: $"{message.From!.FirstName}{message.From.LastName?.Let(lastName => " " + lastName)}",
							question: commandArgument,
							cancellationToken: cancellationToken
						);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: WebUtility.HtmlEncode(result),
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
						if (message.Chat.Type == ChatType.Private) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>Anda terlalu banyak menggunakan /ask. Coba lagi {cooldown}.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>Anda terlalu banyak menggunakan /ask di sini. Coba lagi {cooldown} atau lanjutkan di private chat.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								replyMarkup: new InlineKeyboardMarkup(
									InlineKeyboardButton.WithUrl("Private chat 💬", "t.me/TeknumBot")
								),
								cancellationToken: cancellationToken);
						}
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
						(message.Chat.Type == ChatType.Private
							? ASK_PRIVATE_RATE_LIMITER
							: ASK_GROUP_RATE_LIMITER
						).ValidateActionRate(message.Chat.Id, message.From!.Id);
						string result = await serviceProvider.GetRequiredService<AssistantBot>().AskSomethingAsync(
							name: $"{message.From!.FirstName}{message.From.LastName?.Let(lastName => " " + lastName)}",
							question: repliedToMessage,
							cancellationToken: cancellationToken
						);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: WebUtility.HtmlEncode(result),
							parseMode: ParseMode.Html,
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
					} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
						if (message.Chat.Type == ChatType.Private) {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>Anda terlalu banyak menggunakan /ask. Coba lagi {cooldown}.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>Anda terlalu banyak menggunakan /ask di sini. Coba lagi {cooldown} atau lanjutkan di private chat.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								replyMarkup: new InlineKeyboardMarkup(
									InlineKeyboardButton.WithUrl("Private chat 💬", "t.me/TeknumBot")
								),
								cancellationToken: cancellationToken);
						}
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
						text: "Untuk bertanya, silahkan ketik /ask diikuti pertanyaan.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
		}

		private static readonly RateLimiter TRANSLATE_RATE_LIMITER = RateLimiter.PerUserPerChat(5, TimeSpan.FromMinutes(2));
		public static async Task TranslateAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, string languagePair, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					try {
						TRANSLATE_RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);
						string result = await serviceProvider.GetRequiredService<Translator>().TranslateAsync(
							sentence: commandArgument,
							languagePair: languagePair,
							cancellationToken: cancellationToken
						);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: WebUtility.HtmlEncode(result),
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>Anda terlalu banyak menggunakan penerjemah. Coba lagi {cooldown}.</code>",
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
						TRANSLATE_RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);
						string result = await serviceProvider.GetRequiredService<Translator>().TranslateAsync(
							sentence: repliedToMessage,
							languagePair: languagePair,
							cancellationToken: cancellationToken
						);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: WebUtility.HtmlEncode(result),
							parseMode: ParseMode.Html,
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
					} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>Anda terlalu banyak menggunakan penerjemah. Coba lagi {cooldown}.</code>",
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
				} else {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: $"Untuk menerjemahkan, silahkan ketik /{languagePair} diikuti kalimat.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
		}

		private static readonly RateLimiter CODEGEN_RATE_LIMITER = RateLimiter.PerUserPerChatPerDay(4);
		public static async Task GenerateJavaScriptCodeAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					try {
						CODEGEN_RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);
						string result = await serviceProvider.GetRequiredService<CodeGenerator>().GenerateJavaScriptCodeAsync(
							instructions: commandArgument,
							cancellationToken: cancellationToken
						);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>{WebUtility.HtmlEncode(result)}</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>Anda terlalu banyak menggunakan code generator. Coba lagi {cooldown}.</code>",
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
						CODEGEN_RATE_LIMITER.ValidateActionRate(message.Chat.Id, message.From!.Id);
						string result = await serviceProvider.GetRequiredService<CodeGenerator>().GenerateJavaScriptCodeAsync(
							instructions: repliedToMessage,
							cancellationToken: cancellationToken
						);
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>{WebUtility.HtmlEncode(result)}</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
					} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>Anda terlalu banyak menggunakan code generator. Coba lagi {cooldown}.</code>",
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
				} else {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: $"Untuk generate kode JavaScript, silahkan ketik /genjs diikuti instruksi.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
		}

		private static readonly RateLimiter CHAT_GROUP_RATE_LIMITER = RateLimiter.PerUserPerChat(5, TimeSpan.FromMinutes(15));
		private static readonly RateLimiter CHAT_PRIVATE_RATE_LIMITER = RateLimiter.PerUser(20, TimeSpan.FromMinutes(15));
		public static async Task<Message?> ChatAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, string callSign, CancellationToken cancellationToken) {
			if (message.Text!.StartsWith(callSign, out string? s)
				&& s.TrimStart() is string { Length: > 0 } chatMessage) {
				try {
					(message.Chat.Type == ChatType.Private
						? CHAT_PRIVATE_RATE_LIMITER
						: CHAT_GROUP_RATE_LIMITER
					).ValidateActionRate(message.Chat.Id, message.From!.Id);
					string result = await serviceProvider.GetRequiredService<ConversationBot>().ChatAsync(
						name: $"{message.From!.FirstName}{message.From.LastName?.Let(lastName => " " + lastName)}",
						question: chatMessage,
						cancellationToken: cancellationToken
					);
					return await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: WebUtility.HtmlEncode(result),
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
					if (message.Chat.Type == ChatType.Private) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>Anda terlalu banyak memanggil AI. Coba lagi {cooldown}.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} else {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>Anda terlalu banyak memanggil AI di sini. Coba lagi {cooldown} atau lanjutkan di private chat.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							replyMarkup: new InlineKeyboardMarkup(
								InlineKeyboardButton.WithUrl("Private chat 💬", "t.me/TeknumBot")
							),
							cancellationToken: cancellationToken);
					}
				} catch (OperationCanceledException) {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: "<code>Timeout exceeded.</code>",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
			return null;
		}

		public static async Task<Message?> RespondToThreadAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, ImmutableList<(string Sender, string Text)> thread, CancellationToken cancellationToken) {
			try {
				(message.Chat.Type == ChatType.Private
					? CHAT_PRIVATE_RATE_LIMITER
					: CHAT_GROUP_RATE_LIMITER
				).ValidateActionRate(message.Chat.Id, message.From!.Id);
				string result = await serviceProvider.GetRequiredService<ConversationBot>().RespondToThreadAsync(
					name: $"{message.From!.FirstName}{message.From.LastName?.Let(lastName => " " + lastName)}",
					question: message.Text!,
					thread: thread,
					cancellationToken: cancellationToken
				);
				return await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: WebUtility.HtmlEncode(result),
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
				if (message.Chat.Type == ChatType.Private) {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: $"<code>Anda terlalu banyak memanggil AI. Coba lagi {cooldown}.</code>",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				} else {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: $"<code>Anda terlalu banyak memanggil AI di sini. Coba lagi {cooldown} atau lanjutkan di private chat.</code>",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						replyMarkup: new InlineKeyboardMarkup(
							InlineKeyboardButton.WithUrl("Private chat 💬", "t.me/TeknumBot")
						),
						cancellationToken: cancellationToken);
				}
			} catch (OperationCanceledException) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "<code>Timeout exceeded.</code>",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			}
			return null;
		}
	}
}
