using System.Threading;
using System.Threading.Tasks;
using BotNet.Commands.Soundtrack;
using BotNet.Services.Soundtrack;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Soundtrack {
	public sealed class SoundtrackCommandHandler(
		ITelegramBotClient telegramBotClient,
		SoundtrackProvider soundtrackProvider
	) : ICommandHandler<SoundtrackCommand> {
		public async Task Handle(SoundtrackCommand command, CancellationToken cancellationToken) {
			(SoundtrackSite first, SoundtrackSite second) = soundtrackProvider.GetHourlyPicks();

			string message = $"""
				🎵 <b>This hour's coding soundtracks:</b>

				1️⃣ <b>{first.Name}</b>
				{first.Url}

				2️⃣ <b>{second.Name}</b>
				{second.Url}

				💡 <i>New picks every hour!</i>
				""";

			await telegramBotClient.SendMessage(
				chatId: command.Command.Chat.Id,
				text: message,
				parseMode: ParseMode.Html,
				replyParameters: new() {
					MessageId = command.Command.MessageId
				},
				cancellationToken: cancellationToken
			);
		}
	}
}
