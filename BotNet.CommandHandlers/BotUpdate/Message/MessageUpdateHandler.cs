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

		public async Task Handle(MessageUpdate update, CancellationToken cancellationToken) {
			// Handle slash commands
			if (update.Message.Entities?.FirstOrDefault() is {
				Type: MessageEntityType.BotCommand,
				Offset: 0
			}) {
				if (SlashCommand.TryCreate(
					message: update.Message,
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
				message: update.Message,
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

			// Handle AI follow up message
			if (update.Message is {
				ReplyToMessage.MessageId: int replyToMessageId,
				Chat.Id: long chatId
			} && _telegramMessageCache.GetOrDefault(
				messageId: replyToMessageId,
				chatId: chatId
			) is AIResponseMessage) {
				if (!AIFollowUpMessage.TryCreate(
					message: update.Message,
					thread: _telegramMessageCache.GetThread(
						messageId: replyToMessageId,
						chatId: chatId
					),
					out AIFollowUpMessage? aiFollowUpMessage
				)) {
					return;
				}

				// Cache follow up message
				_telegramMessageCache.Add(
					message: aiFollowUpMessage
				);

				await _commandQueue.DispatchAsync(aiFollowUpMessage);
			}
		}
	}
}
