using System;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.ThisXDoesNotExist;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace BotNet.Services.BotCommands {
	public static class Waifu {
		public static async Task GetRandomWaifuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken) {
			Uri randomUrl = ThisWaifuDoesNotExist.GetRandomUrl();

			await botClient.SendPhotoAsync(
				chatId: message.Chat.Id,
				photo: new InputOnlineFile(randomUrl),
				cancellationToken: cancellationToken);
		}
	}
}
