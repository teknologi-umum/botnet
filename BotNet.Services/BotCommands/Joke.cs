using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.ProgrammerHumor;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace BotNet.Services.BotCommands {
	public static class Joke {
		public static async Task GetRandomJokeAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			(string title, byte[] image) = await serviceProvider.GetRequiredService<ProgrammerHumorScraper>().GetRandomJokeAsync(cancellationToken);
			using MemoryStream imageStream = new(image);

			await botClient.SendPhotoAsync(
				chatId: message.Chat.Id,
				photo: new InputOnlineFile(imageStream, "joke.webp"),
				caption: title,
				cancellationToken: cancellationToken);
		}
	}
}
