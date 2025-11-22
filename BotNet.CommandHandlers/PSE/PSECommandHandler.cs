using System.Collections.Immutable;
using Mediator;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Commands.PSE;
using BotNet.Services.PSE;
using BotNet.Services.PSE.JsonModels;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.PSE {
	public sealed class PSECommandHandler(
		ITelegramBotClient telegramBotClient,
		PSECrawler pseCrawler,
		ILogger<PSECommandHandler> logger
	) : ICommandHandler<PSECommand> {
		private static readonly RateLimiter RateLimiter = RateLimiter.PerUserPerChat(3, TimeSpan.FromMinutes(2));

		public async ValueTask<Unit> Handle(PSECommand command, CancellationToken cancellationToken) {
			try {
				RateLimiter.ValidateActionRate(command.Chat.Id, command.Sender.Id);
			} catch (RateLimitExceededException exc) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"Coba lagi {exc.Cooldown}.",
					parseMode: ParseMode.Html,
					replyParameters: new Telegram.Bot.Types.ReplyParameters { MessageId = command.CommandMessageId },
					cancellationToken: cancellationToken
				);
				return default;
			}
			ImmutableList<DigitalService> result = await pseCrawler.SearchAsync(command.Keyword, take: 5, cancellationToken);

			if (result.IsEmpty) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: "Sistem Elektronik tidak ditemukan.",
					parseMode: ParseMode.Html,
					replyParameters: new Telegram.Bot.Types.ReplyParameters { MessageId = command.CommandMessageId },
					cancellationToken: cancellationToken
				);
				return default;
			}

			string text = string.Join("\n",
				from digitalService in result
				select $"<b>{WebUtility.HtmlEncode(digitalService.NamaSe)} ({WebUtility.HtmlEncode(digitalService.PseName)})</b>\n"
					+ $"🔗 {WebUtility.HtmlEncode(digitalService.Domain)}\n"
					+ $"📋 {WebUtility.HtmlEncode(digitalService.NomorTdpse)}\n"
					+ $"📅 {WebUtility.HtmlEncode(digitalService.TanggalTerdaftar)}\n"
					+ $"{(digitalService.IsDomestik ? "🇮🇩 Domestik" : "🌐 Asing")}\n"
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
	return default;
		}
	}
}
