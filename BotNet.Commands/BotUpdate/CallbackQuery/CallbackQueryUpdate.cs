namespace BotNet.Commands.BotUpdate.CallbackQuery {
	public sealed record CallbackQueryUpdate(
		Telegram.Bot.Types.CallbackQuery CallbackQuery
	) : ICommand;
}
