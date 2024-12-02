using System.Net;
using BotNet.Commands.Khodam;
using BotNet.Services.Khodam;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Khodam {
	public sealed class KhodamCommandHandler(
		ITelegramBotClient telegramBotClient
	) : ICommandHandler<KhodamCommand> {
		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;

		public async Task Handle(KhodamCommand command, CancellationToken cancellationToken) {
			string khodam = KhodamCalculator.CalculateKhodam(
				name: command.Name,
				userId: command.UserId
			);

			await _telegramBotClient.SendMessage(
				chatId: command.Chat.Id,
				text: $$"""
					Khodam <b>{{WebUtility.HtmlEncode(command.Name)}}</b> hari ini adalah...
					<b>{{khodam}}</b>
					""",
				parseMode: ParseMode.Html,
				replyParameters: new ReplyParameters { MessageId = command.TargetMessageId },
				cancellationToken: cancellationToken
			);
		}
	}
}
