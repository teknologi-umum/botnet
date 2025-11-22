using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Commands.PSE;
using BotNet.Services.PSE;
using BotNet.Services.PSE.JsonModels;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.PSE {
	public sealed class PSECommandHandler(
		ITelegramBotClient telegramBotClient,
		PSECrawler pseCrawler,
		ILogger<PSECommandHandler> logger
	) : ICommandHandler<PSECommand> {
		public async Task Handle(PSECommand command, CancellationToken cancellationToken) {
			ImmutableList<(Domicile Domicile, DigitalService DigitalService)> result = pseCrawler.Search(command.Keyword, take: 5);

			if (result.IsEmpty) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: "Sistem Elektronik tidak ditemukan.",
					parseMode: ParseMode.Html,
					replyParameters: new Telegram.Bot.Types.ReplyParameters { MessageId = command.CommandMessageId },
					cancellationToken: cancellationToken
				);
				return;
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

			await telegramBotClient.SendMessage(
				chatId: command.Chat.Id,
				text: text,
				parseMode: ParseMode.Html,
				replyParameters: new Telegram.Bot.Types.ReplyParameters { MessageId = command.CommandMessageId },
				cancellationToken: cancellationToken
			);
		}
	}
}
