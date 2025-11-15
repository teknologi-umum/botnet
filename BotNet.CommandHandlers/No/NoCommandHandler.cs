using BotNet.Commands.No;
using BotNet.Services.NoAsAService;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.No {
	public sealed class NoCommandHandler(
		ITelegramBotClient telegramBotClient,
		NoAsAServiceClient noAsAServiceClient
	) : ICommandHandler<NoCommand> {
		public async Task Handle(NoCommand command, CancellationToken cancellationToken) {
			string reason = await noAsAServiceClient.GetNoReasonAsync(cancellationToken);

			await telegramBotClient.SendMessage(
				chatId: command.Command.Chat.Id,
				text: $"❌ {reason}",
				parseMode: ParseMode.Html,
				replyParameters: new ReplyParameters {
					MessageId = command.Command.MessageId
				},
				cancellationToken: cancellationToken
			);
		}
	}
}
