using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.PSE;
using BotNet.Services.PSE.JsonModels;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.BotCommands {
	public static class PSE {
		public static async Task SearchAsync(ITelegramBotClient botClient, IServiceProvider serviceProvider, Message message, CancellationToken cancellationToken) {
			if (message.Entities?.FirstOrDefault() is { Type: MessageEntityType.BotCommand, Offset: 0, Length: int commandLength }
				&& message.Text![commandLength..].Trim() is string commandArgument
				&& commandArgument.Length > 0) {
				PSECrawler pseCrawler = serviceProvider.GetRequiredService<PSECrawler>();
				ImmutableList<(Domicile Domicile, DigitalService DigitalService)> result = pseCrawler.Search(commandArgument, take: 5);

				if (result.IsEmpty) {
					await botClient.SendTextMessageAsync(
						chatId: message.Chat.Id,
						text: "Sistem Elektronik tidak ditemukan.",
						parseMode: ParseMode.Html,
						replyToMessageId: message.MessageId,
						cancellationToken: cancellationToken);
				}

				string text = string.Join("\n",
					from r in result
					let domicile = r.Domicile
					let digitalService = r.DigitalService
					select $"<b>{digitalService.Attributes.Name} ({digitalService.Attributes.CompanyName})</b>\n"
					+ $"🔗 {digitalService.Attributes.Website}\n"
					+ $"{digitalService.Attributes.Status.ToStatusEmoji()} {domicile.ToFriendlyDomicile()}, {digitalService.Attributes.Status.ToFriendlyStatus()}\n"
				);

				if (result.Count == 5) {
					text += "\nBot ini hanya menampilkan maksimal 5 sistem elektronik.";
				}

				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: text,
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			} else {
				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: "Untuk mencari sistem elektronik, silahkan ketik /pse diikuti keyword.",
					parseMode: ParseMode.Html,
					replyToMessageId: message.MessageId,
					cancellationToken: cancellationToken);
			}
		}
	}
}
