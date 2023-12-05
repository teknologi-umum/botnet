using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.MarkdownV2;
using BotNet.Services.OpenAI;
using BotNet.Services.OpenAI.Models;
using BotNet.Services.OpenAI.Skills;
using BotNet.Services.RateLimit;
using BotNet.Services.Stability.Models;
using Microsoft.Extensions.DependencyInjection;
using RG.Ninja;
using SkiaSharp;
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
						text: "Untuk explain code, silakan ketik /explain diikuti code.",
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
						text: "Untuk bertanya, silakan ketik /ask diikuti pertanyaan.",
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
						text: $"Untuk menerjemahkan, silakan ketik /{languagePair} diikuti kalimat.",
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
						text: $"Untuk generate kode JavaScript, silakan ketik /genjs diikuti instruksi.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
		}

		private static readonly RateLimiter CHAT_GROUP_RATE_LIMITER = RateLimiter.PerUserPerChat(5, TimeSpan.FromMinutes(15));
		private static readonly RateLimiter CHAT_PRIVATE_RATE_LIMITER = RateLimiter.PerUser(20, TimeSpan.FromMinutes(15));
		[Obsolete("Use StreamChatWithFriendlyBotAsync instead.", error: true)]
		public static async Task<Message?> ChatWithFriendlyBotAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, string callSign, CancellationToken cancellationToken) {
			if (message.Text!.StartsWith(callSign, out string? s)
				&& s[1..].TrimStart() is string { Length: > 0 } chatMessage) {
				try {
					(message.Chat.Type == ChatType.Private
						? CHAT_PRIVATE_RATE_LIMITER
						: CHAT_GROUP_RATE_LIMITER
					).ValidateActionRate(message.Chat.Id, message.From!.Id);
					string result = await serviceProvider.GetRequiredService<FriendlyBot>().ChatAsync(
						message: chatMessage,
						cancellationToken: cancellationToken
					);
					ImmutableList<Uri> attachments = serviceProvider.GetRequiredService<AttachmentGenerator>().GenerateAttachments(result);
					if (attachments.Count == 0) {
						return await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: MarkdownV2Sanitizer.Sanitize(result),
							parseMode: ParseMode.MarkdownV2,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} else if (attachments.Count == 1) {
						return await botClient.SendPhotoAsync(
							chatId: message.Chat.Id,
							photo: new InputFileUrl(attachments[0]),
							caption: MarkdownV2Sanitizer.Sanitize(result),
							parseMode: ParseMode.MarkdownV2,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} else {
						Message sentMessage = await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: MarkdownV2Sanitizer.Sanitize(result),
							parseMode: ParseMode.MarkdownV2,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
						await botClient.SendMediaGroupAsync(
							chatId: message.Chat.Id,
							media: from attachment in attachments
								   select new InputMediaPhoto(new InputFileUrl(attachment.OriginalString)),
							cancellationToken: cancellationToken);
						return sentMessage;
					}
				} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
					if (message.Chat.Type == ChatType.Private) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>Anda terlalu banyak memanggil {callSign}. Coba lagi {cooldown}.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} else {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>Anda terlalu banyak memanggil {callSign} di sini. Coba lagi {cooldown} atau lanjutkan di private chat.</code>",
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

		[Obsolete("Use StreamChatWithFriendlyBotAsync instead.", error: true)]
		public static async Task<Message?> ChatWithFriendlyBotAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, ImmutableList<(string Sender, string? Text, string? ImageBase64)> thread, CancellationToken cancellationToken) {
			try {
				(message.Chat.Type == ChatType.Private
					? CHAT_PRIVATE_RATE_LIMITER
					: CHAT_GROUP_RATE_LIMITER
				).ValidateActionRate(message.Chat.Id, message.From!.Id);
				string result = await serviceProvider.GetRequiredService<FriendlyBot>().ChatAsync(
					message: message.Text!,
					thread: thread,
					cancellationToken: cancellationToken
				);
				ImmutableList<Uri> attachments = serviceProvider.GetRequiredService<AttachmentGenerator>().GenerateAttachments(result);
				if (attachments.Count == 0) {
					return await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: MarkdownV2Sanitizer.Sanitize(result),
						parseMode: ParseMode.MarkdownV2,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				} else if (attachments.Count == 1) {
					return await botClient.SendPhotoAsync(
						chatId: message.Chat.Id,
						photo: new InputFileUrl(attachments[0]),
						caption: MarkdownV2Sanitizer.Sanitize(result),
						parseMode: ParseMode.MarkdownV2,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				} else {
					Message sentMessage = await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: MarkdownV2Sanitizer.Sanitize(result),
						parseMode: ParseMode.MarkdownV2,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
					await botClient.SendMediaGroupAsync(
						chatId: message.Chat.Id,
						media: from attachment in attachments
							   select new InputMediaPhoto(new InputFileUrl(attachment.OriginalString)),
						cancellationToken: cancellationToken);
					return sentMessage;
				}
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

		public static async Task<Message?> ChatWithSarcasticBotAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, string callSign, CancellationToken cancellationToken) {
			if (message.Text!.StartsWith(callSign, out string? s)
				&& s.TrimStart() is string { Length: > 0 } chatMessage) {
				try {
					(message.Chat.Type == ChatType.Private
						? CHAT_PRIVATE_RATE_LIMITER
						: CHAT_GROUP_RATE_LIMITER
					).ValidateActionRate(message.Chat.Id, message.From!.Id);
					string result = await serviceProvider.GetRequiredService<SarcasticBot>().ChatAsync(
						callSign: callSign,
						name: $"{message.From!.FirstName}{message.From.LastName?.Let(lastName => " " + lastName)}",
						question: chatMessage,
						cancellationToken: cancellationToken
					);
					ImmutableList<Uri> attachments = serviceProvider.GetRequiredService<AttachmentGenerator>().GenerateAttachments(result);
					if (attachments.Count == 0) {
						return await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: WebUtility.HtmlEncode(result),
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} else if (attachments.Count == 1) {
						return await botClient.SendPhotoAsync(
							chatId: message.Chat.Id,
							photo: new InputFileUrl(attachments[0]),
							caption: WebUtility.HtmlEncode(result),
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} else {
						Message sentMessage = await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: WebUtility.HtmlEncode(result),
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
						await botClient.SendMediaGroupAsync(
							chatId: message.Chat.Id,
							media: from attachment in attachments
								   select new InputMediaPhoto(new InputFileUrl(attachment.OriginalString)),
							cancellationToken: cancellationToken);
						return sentMessage;
					}
				} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
					if (message.Chat.Type == ChatType.Private) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>Anda terlalu banyak memanggil Pakde. Coba lagi {cooldown}.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
					} else {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>Anda terlalu banyak memanggil Pakde di sini. Coba lagi {cooldown} atau lanjutkan di private chat.</code>",
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

		public static async Task<Message?> ChatWithSarcasticBotAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, ImmutableList<(string Sender, string? Text, string? ImageBase64)> thread, string callSign, CancellationToken cancellationToken) {
			try {
				(message.Chat.Type == ChatType.Private
					? CHAT_PRIVATE_RATE_LIMITER
					: CHAT_GROUP_RATE_LIMITER
				).ValidateActionRate(message.Chat.Id, message.From!.Id);
				string result = await serviceProvider.GetRequiredService<SarcasticBot>().RespondToThreadAsync(
					callSign: callSign,
					name: $"{message.From!.FirstName}{message.From.LastName?.Let(lastName => " " + lastName)}",
					question: message.Text!,
					thread: thread,
					cancellationToken: cancellationToken
				);
				ImmutableList<Uri> attachments = serviceProvider.GetRequiredService<AttachmentGenerator>().GenerateAttachments(result);
				if (attachments.Count == 0) {
					return await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: WebUtility.HtmlEncode(result),
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				} else if (attachments.Count == 1) {
					return await botClient.SendPhotoAsync(
						chatId: message.Chat.Id,
						photo: new InputFileUrl(attachments[0]),
						caption: WebUtility.HtmlEncode(result),
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				} else {
					Message sentMessage = await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: WebUtility.HtmlEncode(result),
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
					await botClient.SendMediaGroupAsync(
						chatId: message.Chat.Id,
						media: from attachment in attachments
							   select new InputMediaPhoto(new InputFileUrl(attachment.OriginalString)),
						cancellationToken: cancellationToken);
					return sentMessage;
				}
			} catch (RateLimitExceededException exc) when (exc is { Cooldown: var cooldown }) {
				if (message.Chat.Type == ChatType.Private) {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: $"<code>Anda terlalu banyak memanggil Pakde. Coba lagi {cooldown}.</code>",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				} else {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: $"<code>Anda terlalu banyak memanggil Pakde di sini. Coba lagi {cooldown} atau lanjutkan di private chat.</code>",
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

		private static readonly RateLimiter TLDR_GROUP_RATE_LIMITER = RateLimiter.PerUserPerChat(3, TimeSpan.FromMinutes(5));
		private static readonly RateLimiter TLDR_PRIVATE_RATE_LIMITER = RateLimiter.PerUser(10, TimeSpan.FromMinutes(5));
		public static async Task GenerateTldrAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					try {
						(message.Chat.Type == ChatType.Private
							? TLDR_PRIVATE_RATE_LIMITER
							: TLDR_GROUP_RATE_LIMITER
						).ValidateActionRate(message.Chat.Id, message.From!.Id);
						string result = await serviceProvider.GetRequiredService<TldrGenerator>().GenerateTldrAsync(commandArgument, cancellationToken);
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
								text: $"<code>Anda terlalu banyak menggunakan /tldr. Coba lagi {cooldown}.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>Anda terlalu banyak menggunakan /tldr di sini. Coba lagi {cooldown} atau lanjutkan di private chat.</code>",
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
							? TLDR_PRIVATE_RATE_LIMITER
							: TLDR_GROUP_RATE_LIMITER
						).ValidateActionRate(message.Chat.Id, message.From!.Id);
						string result = await serviceProvider.GetRequiredService<TldrGenerator>().GenerateTldrAsync(repliedToMessage, cancellationToken);
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
								text: $"<code>Anda terlalu banyak menggunakan /tldr. Coba lagi {cooldown}.</code>",
								parseMode: ParseMode.Html,
								replyToMessageId: message.MessageId,
								cancellationToken: cancellationToken);
						} else {
							await botClient.SendTextMessageAsync(
								chatId: message.Chat.Id,
								text: $"<code>Anda terlalu banyak menggunakan /tldr di sini. Coba lagi {cooldown} atau lanjutkan di private chat.</code>",
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
						text: "Untuk generate TLDR, silakan ketik /tldr diikuti artikel.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}
			}
		}

		private static readonly RateLimiter IMAGE_GENERATION_PER_USER_RATE_LIMITER = RateLimiter.PerUser(1, TimeSpan.FromMinutes(5));
		private static readonly RateLimiter IMAGE_GENERATION_PER_CHAT_RATE_LIMITER = RateLimiter.PerChat(2, TimeSpan.FromMinutes(3));
		public static async Task StreamChatWithFriendlyBotAsync(
			ITelegramBotClient botClient,
			IServiceProvider serviceProvider,
			Message message,
			CancellationToken cancellationToken
		) {
			try {
				(message.Chat.Type == ChatType.Private
					? CHAT_PRIVATE_RATE_LIMITER
					: CHAT_GROUP_RATE_LIMITER
				).ValidateActionRate(message.Chat.Id, message.From!.Id);

				string? fileId;
				string? prompt;
				if (message is { Photo.Length: > 0, Caption: { } }) {
					fileId = message.Photo.OrderByDescending(photoSize => photoSize.Width).First().FileId;
					prompt = message.Caption;
				} else if (message.ReplyToMessage is { Photo.Length: > 0 }
					&& message.Text is { }) {
					fileId = message.ReplyToMessage.Photo.OrderByDescending(photoSize => photoSize.Width).First().FileId;
					prompt = message.Text;
				} else if (message.ReplyToMessage is { Sticker: { } }
					&& message.Text is { }) {
					fileId = message.ReplyToMessage.Sticker.FileId;
					prompt = message.Text;
				} else {
					fileId = null;
					prompt = null;
				}

				if (fileId != null && prompt != null) {
					(string? imageBase64, string? error) = await GetImageBase64Async(botClient, fileId, cancellationToken);
					if (error != null) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: $"<code>{error}</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
						return;
					}
					await serviceProvider.GetRequiredService<VisionBot>().StreamChatAsync(
						message: prompt,
						imageBase64: imageBase64!,
						chatId: message.Chat.Id,
						replyToMessageId: message.MessageId
					);
				} else {
					IntentDetector intentDetector = serviceProvider.GetRequiredService<IntentDetector>();
					ChatIntent chatIntent = await intentDetector.DetectChatIntentAsync(
						message: message.Text!,
						cancellationToken: cancellationToken
					);

					switch (chatIntent) {
						case ChatIntent.Question:
							await serviceProvider.GetRequiredService<FriendlyBot>().StreamChatAsync(
								message: message.Text!,
								chatId: message.Chat.Id,
								replyToMessageId: message.MessageId
							);
							break;
						case ChatIntent.ImageGeneration: {
								IMAGE_GENERATION_PER_USER_RATE_LIMITER.ValidateActionRate(
									chatId: message.Chat.Id,
									userId: message.From.Id
								);
								IMAGE_GENERATION_PER_CHAT_RATE_LIMITER.ValidateActionRate(
									chatId: message.Chat.Id,
									userId: message.From.Id
								);
								Message busyMessage = await botClient.SendTextMessageAsync(
									chatId: message.Chat.Id,
									text: "Generating image… ⏳",
									parseMode: ParseMode.Markdown,
									replyToMessageId: message.MessageId,
									cancellationToken: cancellationToken
								);
								//Uri generatedImageUrl = await serviceProvider.GetRequiredService<ImageGenerationBot>().GenerateImageAsync(
								//	prompt: message.Text!,
								//	cancellationToken: cancellationToken
								//);
								try {
									byte[] generatedImage = await serviceProvider.GetRequiredService<Stability.Skills.ImageGenerationBot>().GenerateImageAsync(
										prompt: message.Text!,
										cancellationToken: cancellationToken
									);
									using MemoryStream generatedImageStream = new(generatedImage);
									try {
										await botClient.DeleteMessageAsync(
											chatId: busyMessage.Chat.Id,
											messageId: busyMessage.MessageId,
											cancellationToken: cancellationToken
										);
									} catch (OperationCanceledException) {
										throw;
									}

									Message generatedImageMessage = await botClient.SendPhotoAsync(
										chatId: message.Chat.Id,
										photo: new InputFileStream(generatedImageStream, "art.png"),
										replyToMessageId: message.MessageId,
										cancellationToken: cancellationToken
									);

									// Track generated image for continuation
									serviceProvider.GetRequiredService<ThreadTracker>().TrackMessage(
										messageId: generatedImageMessage.MessageId,
										sender: "AI",
										text: null,
										imageBase64: Convert.ToBase64String(generatedImage),
										replyToMessageId: message.MessageId
									);
								} catch (ContentFilteredException exc) {
									await botClient.EditMessageTextAsync(
										chatId: busyMessage.Chat.Id,
										messageId: busyMessage.MessageId,
										text: $"<code>{exc.Message ?? "Content filtered."}</code>",
										parseMode: ParseMode.Html
									);
								} catch {
									await botClient.EditMessageTextAsync(
										chatId: busyMessage.Chat.Id,
										messageId: busyMessage.MessageId,
										text: "<code>Failed to generate image.</code>",
										parseMode: ParseMode.Html
									);
								}
								break;
							}
					}
				}
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
			} catch (HttpRequestException exc) when (exc.StatusCode == HttpStatusCode.TooManyRequests) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "<code>Too many requests.</code>",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			} catch (HttpRequestException exc) {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "<code>Unknown error.</code>",
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
		}

		public static async Task StreamChatWithFriendlyBotAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, ImmutableList<(string Sender, string? Text, string? ImageBase64)> thread, CancellationToken cancellationToken) {
			try {
				(message.Chat.Type == ChatType.Private
					? CHAT_PRIVATE_RATE_LIMITER
					: CHAT_GROUP_RATE_LIMITER
				).ValidateActionRate(message.Chat.Id, message.From!.Id);

				if (thread.FirstOrDefault().ImageBase64 is { } imageBase64) {
					await serviceProvider.GetRequiredService<VisionBot>().StreamChatAsync(
						message: message.Text!,
						imageBase64: imageBase64,
						chatId: message.Chat.Id,
						replyToMessageId: message.MessageId
					);
				} else {
					await serviceProvider.GetRequiredService<FriendlyBot>().StreamChatAsync(
						message: message.Text!,
						thread: thread,
						chatId: message.Chat.Id,
						replyToMessageId: message.MessageId
					);
				}
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

		private static async Task<(string? ImageBase64, string? Error)> GetImageBase64Async(ITelegramBotClient botClient, string fileId, CancellationToken cancellationToken) {
			// Download photo
			using MemoryStream originalImageStream = new();
			await botClient.GetInfoAndDownloadFileAsync(
				fileId: fileId,
				destination: originalImageStream,
				cancellationToken: cancellationToken);
			byte[] originalImage = originalImageStream.ToArray();

			// Limit input image to 200KB
			if (originalImage.Length > 200 * 1024) {
				return (null, "Image larger than 200KB");
			}

			// Decode image
			originalImageStream.Position = 0;
			using SKCodec codec = SKCodec.Create(originalImageStream, out SKCodecResult codecResult);
			if (codecResult != SKCodecResult.Success) {
				return (null, "Invalid image");
			}

			if (codec.EncodedFormat != SKEncodedImageFormat.Jpeg
				&& codec.EncodedFormat != SKEncodedImageFormat.Webp) {
				return (null, "Image must be compressed image");
			}
			SKBitmap bitmap = SKBitmap.Decode(codec);

			// Limit input image to 1280x1280
			if (bitmap.Width > 1280 || bitmap.Width > 1280) {
				return (null, "Image larger than 1280x1280");
			}

			// Handle stickers
			if (codec.EncodedFormat == SKEncodedImageFormat.Webp) {
				SKImage image = SKImage.FromBitmap(bitmap);
				SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 20);
				using MemoryStream jpegStream = new();
				data.SaveTo(jpegStream);

				// Encode image as base64
				return (Convert.ToBase64String(jpegStream.ToArray()), null);
			}

			// Encode image as base64
			return (Convert.ToBase64String(originalImage), null);
		}
	}
}
