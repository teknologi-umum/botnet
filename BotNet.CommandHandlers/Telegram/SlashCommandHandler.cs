using BotNet.Commands;
using BotNet.Commands.Common;
using BotNet.Commands.Eval;
using BotNet.Commands.Exec;
using BotNet.Commands.FlipFlop;
using BotNet.Commands.Fuck;
using BotNet.Commands.Telegram;
using Telegram.Bot;

namespace BotNet.CommandHandlers.Telegram {
	public sealed class SlashCommandHandler(
		ITelegramBotClient telegramBotClient,
		ICommandQueue commandQueue,
		ITelegramMessageCache telegramMessageCache
	) : ICommandHandler<SlashCommand> {
		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly ICommandQueue _commandQueue = commandQueue;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;

		public async Task Handle(SlashCommand command, CancellationToken cancellationToken) {
			try {
				switch (command.Command) {
					case "/flip":
					case "/flop":
					case "/flep":
					case "/flap":
						await _commandQueue.DispatchAsync(FlipFlopCommand.FromSlashCommand(command));
						break;
					case "/evaljs":
					case "/evalcs":
						await _commandQueue.DispatchAsync(EvalCommand.FromSlashCommand(command));
						break;
					case "/fuck":
						await _commandQueue.DispatchAsync(FuckCommand.FromSlashCommand(command));
						break;
					case "/c":
					case "/clojure":
					case "/crystal":
					case "/dart":
					case "/elixir":
					case "/go":
					case "/java":
					case "/kotlin":
					case "/lua":
					case "/pascal":
					case "/php":
					case "/python":
					case "/ruby":
					case "/rust":
					case "/scala":
					case "/swift":
					case "/julia":
					case "/sqlite3":
					case "/commonlisp":
					case "/cpp":
					case "/cs":
					case "/fs":
					case "/js":
					case "/ts":
					case "/vb":
						await _commandQueue.DispatchAsync(ExecCommand.FromSlashCommand(command));
						break;
				}
			} catch (UsageException exc) {
				await _telegramBotClient.SendTextMessageAsync(
					chatId: command.ChatId,
					text: exc.Message,
					parseMode: exc.ParseMode,
					replyToMessageId: exc.CommandMessageId,
					cancellationToken: cancellationToken
				);
			}
		}
	}
}
