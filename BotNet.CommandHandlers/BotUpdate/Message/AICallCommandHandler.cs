using BotNet.Commands;
using BotNet.Commands.AI.OpenAI;
using BotNet.Commands.BotUpdate.Message;

namespace BotNet.CommandHandlers.BotUpdate.Message {
	public sealed class AICallCommandHandler(
		ICommandQueue commandQueue,
		ITelegramMessageCache telegramMessageCache
	) : ICommandHandler<AICallCommand> {
		private readonly ICommandQueue _commandQueue = commandQueue;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;

		public async Task Handle(AICallCommand command, CancellationToken cancellationToken) {
			switch (command.CallSign) {
				// OpenAI GPT-4 Chat
				case "AI" or "Bot" or "GPT":
					await _commandQueue.DispatchAsync(
						command: OpenAITextPrompt.FromAICallCommand(
							aiCallCommand: command,
							thread: command.ReplyToMessageId.HasValue
								? _telegramMessageCache.GetThread(
									messageId: command.ReplyToMessageId.Value,
									chatId: command.ChatId
								)
								: Enumerable.Empty<MessageBase>()
						)
					);
					break;
			}
		}
	}
}
