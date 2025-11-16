using BotNet.Commands;
using BotNet.Commands.AI.OpenAI;
using BotNet.Commands.Art;
using BotNet.Commands.BMKG;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.Common;
using BotNet.Commands.Eval;
using BotNet.Commands.Exec;
using BotNet.Commands.FlipFlop;
using BotNet.Commands.Fuck;
using BotNet.Commands.GoogleMaps;
using BotNet.Commands.Humor;
using BotNet.Commands.Khodam;
using BotNet.Commands.Movie;
using BotNet.Commands.No;
using BotNet.Commands.Pick;
using BotNet.Commands.Pop;
using BotNet.Commands.Primbon;
using BotNet.Commands.Privilege;
using BotNet.Commands.QrCode;
using BotNet.Commands.Soundtrack;
using BotNet.Commands.TimeZone;
using BotNet.Commands.Weather;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotNet.CommandHandlers.BotUpdate.Message {
	public sealed class SlashCommandHandler(
		ITelegramBotClient telegramBotClient,
		ICommandQueue commandQueue,
		ITelegramMessageCache telegramMessageCache
	) : ICommandHandler<SlashCommand> {
		public async Task Handle(SlashCommand command, CancellationToken cancellationToken) {
			try {
				switch (command.Command) {
					case "/flip":
					case "/flop":
					case "/flep":
					case "/flap":
						await commandQueue.DispatchAsync(FlipFlopCommand.FromSlashCommand(command));
						break;
					case "/evaljs":
					case "/evalcs":
						await commandQueue.DispatchAsync(EvalCommand.FromSlashCommand(command));
						break;
					case "/fuck":
						await commandQueue.DispatchAsync(FuckCommand.FromSlashCommand(command));
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
						await commandQueue.DispatchAsync(ExecCommand.FromSlashCommand(command));
						break;
					case "/pop":
						await commandQueue.DispatchAsync(PopCommand.FromSlashCommand(command));
						break;
					case "/ask":
						await commandQueue.DispatchAsync(
							command: AskCommand.FromSlashCommand(
								command: command,
								thread: command.ReplyToMessage is null
									? []
									: telegramMessageCache.GetThread(
										firstMessage: command.ReplyToMessage
									)
							)
						);
						break;
					case "/humor":
						await commandQueue.DispatchAsync(HumorCommand.FromSlashCommand(command));
						break;
					case "/primbon":
						await commandQueue.DispatchAsync(PrimbonCommand.FromSlashCommand(command));
						break;
					case "/art":
						await commandQueue.DispatchAsync(ArtCommand.FromSlashCommand(command));
						break;
					case "/bmkg":
						await commandQueue.DispatchAsync(BmkgCommand.FromSlashCommand(command));
						break;
					case "/map":
						await commandQueue.DispatchAsync(MapCommand.FromSlashCommand(command));
						break;
					case "/weather":
						await commandQueue.DispatchAsync(WeatherCommand.FromSlashCommand(command));
						break;
					case "/privilege":
					case "/start":
						await commandQueue.DispatchAsync(PrivilegeCommand.FromSlashCommand(command));
						break;
					case "/khodam":
						await commandQueue.DispatchAsync(KhodamCommand.FromSlashCommand(command));
						break;
					case "/no":
						await commandQueue.DispatchAsync(NoCommand.FromSlashCommand(command));
						break;
					case "/soundtrack":
						await commandQueue.DispatchAsync(SoundtrackCommand.FromSlashCommand(command));
						break;
					case "/time":
						await commandQueue.DispatchAsync(TimeCommand.FromSlashCommand(command));
						break;
					case "/qr":
						await commandQueue.DispatchAsync(QrCommand.FromSlashCommand(command));
						break;
					case "/pick":
						await commandQueue.DispatchAsync(PickCommand.FromSlashCommand(command));
						break;
					case "/movie":
						await commandQueue.DispatchAsync(MovieCommand.FromSlashCommand(command));
						break;
				}
			} catch (UsageException exc) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: exc.Message,
					parseMode: exc.ParseMode,
					replyParameters: new ReplyParameters { MessageId = exc.CommandMessageId },
					cancellationToken: cancellationToken
				);
			}
		}
	}
}
