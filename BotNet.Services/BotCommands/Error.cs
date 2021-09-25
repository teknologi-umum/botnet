using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class Error {
		public static async Task HandleErrorAsync(Exception _, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
			await botClient.SendTextMessageAsync(
				chatId: message.Chat.Id,
				text: "Uh oh, something went wrong. Ask the devs to check their logs.",
				parseMode: ParseMode.Html,
				replyToMessageId: message.MessageId,
				cancellationToken: cancellationToken);
		}
	}
}
