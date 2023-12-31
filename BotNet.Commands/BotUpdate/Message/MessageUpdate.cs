namespace BotNet.Commands.BotUpdate.Message {
	public sealed record MessageUpdate(
		Telegram.Bot.Types.Message Message
	) : ICommand;
}
