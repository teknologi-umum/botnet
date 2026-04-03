using System.Globalization;
using BotNet.Commands.Mbg;
using Mediator;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Mbg {
	public sealed class MbgCommandHandler(
		ITelegramBotClient telegramBotClient
	) : ICommandHandler<MbgCommand> {
		public async ValueTask<Unit> Handle(MbgCommand command, CancellationToken cancellationToken) {
			string formattedRupiah = command.RupiahAmount.ToString("N0", CultureInfo.InvariantCulture);
			string mbgTime = MbgCommand.FormatMbgTime(command.RupiahAmount);
			string responseText = $"Rp {formattedRupiah} setara dengan {mbgTime}";

			await telegramBotClient.SendMessage(
				chatId: command.Chat.Id,
				text: responseText,
				parseMode: ParseMode.Html,
				replyParameters: new ReplyParameters {
					MessageId = command.MessageId
				},
				cancellationToken: cancellationToken
			);
			return default;
		}
	}
}
