namespace BotNet.Commands.BotUpdate.InlineQuery {
	public sealed record InlineQueryUpdate(
		Telegram.Bot.Types.InlineQuery InlineQuery
	) : ICommand;
}
