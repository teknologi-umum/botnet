using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.StackExchange;
using BotNet.Services.StackExchange.Models;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Ask {
		public static async Task HandleAskAsync(IServiceProvider serviceProvider, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
			if (message.Entities is { Length: 1 } entities
				&& entities[0] is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument) {
				if (commandArgument.Length > 0) {
					if (await AskSourcesAsync(serviceProvider, commandArgument, cancellationToken) is string answer) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: answer,
							parseMode: ParseMode.Html,
							replyToMessageId: message.MessageId,
							cancellationToken: cancellationToken);
						return;
					}
				} else if (message.ReplyToMessage?.Text is string repliedToMessage) {
					StackExchangeClient stackExchangeClient = serviceProvider.GetRequiredService<StackExchangeClient>();
					ImmutableList<StackExchangeQuestionSnippet> searchResult = await stackExchangeClient.SearchStackOverflowAsync("", repliedToMessage, cancellationToken);
					if (await AskSourcesAsync(serviceProvider, commandArgument, cancellationToken) is string answer) {
						await botClient.SendTextMessageAsync(
							chatId: message.Chat.Id,
							text: answer,
							parseMode: ParseMode.Html,
							replyToMessageId: message.ReplyToMessage.MessageId,
							cancellationToken: cancellationToken);
						return;
					}
				} else {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: "Untuk bertanya, silahkan ketik /ask diikuti pertanyaan.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
					return;
				}
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "Jawaban tidak ditemukan. Cobalah tanya ke /laodeai karena feature /ask ini masih belum sempurna.",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			}
		}

		private static async Task<string?> AskSourcesAsync(IServiceProvider serviceProvider, string query, CancellationToken cancellationToken) {
			string? answer = null;
			if (answer is null) answer = await serviceProvider.GetRequiredService<StackExchangeClient>().TryAskAsync(query, cancellationToken);
			return answer;
		}
	}
}
