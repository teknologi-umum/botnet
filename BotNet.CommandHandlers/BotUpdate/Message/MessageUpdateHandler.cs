using BotNet.Commands;
using BotNet.Commands.BotUpdate.Message;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.BotUpdate.Message {
	public sealed class MessageUpdateHandler(
		ICommandQueue commandQueue,
		ITelegramMessageCache telegramMessageCache
	) : ICommandHandler<MessageUpdate> {
		private readonly ICommandQueue _commandQueue = commandQueue;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;

		public async Task Handle(MessageUpdate command, CancellationToken cancellationToken) {
			// Handle slash commands
			if (command.Message.Entities?.FirstOrDefault() is {
				Type: MessageEntityType.BotCommand,
				Offset: 0
			}) {
				if (SlashCommand.TryCreate(
					message: command.Message,
					out SlashCommand? slashCommand
				)) {
					await _commandQueue.DispatchAsync(
						command: slashCommand
					);
				}
				return;
			}

			// Handle AI calls
			if (AICallCommand.TryCreate(
				message: command.Message,
				out AICallCommand? aiCallCommand
			)) {
				// Cache both message and reply to message
				_telegramMessageCache.Add(
					message: aiCallCommand
				);
				if (aiCallCommand.ReplyToMessage is { } replyToMessage) {
					_telegramMessageCache.Add(
						message: replyToMessage
					);
				}

				await _commandQueue.DispatchAsync(
					command: aiCallCommand
				);
				return;
			}

			// TODO: Handle AI thread replies
		}
	}
}
