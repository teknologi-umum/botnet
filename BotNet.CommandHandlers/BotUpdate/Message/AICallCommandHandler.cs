using BotNet.Commands;
using BotNet.Commands.AI.OpenAI;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Services.OpenAI;

namespace BotNet.CommandHandlers.BotUpdate.Message {
	public sealed class AICallCommandHandler(
		ICommandQueue commandQueue,
		ITelegramMessageCache telegramMessageCache,
		IntentDetector intentDetector
	) : ICommandHandler<AICallCommand> {
		private readonly ICommandQueue _commandQueue = commandQueue;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;
		private readonly IntentDetector _intentDetector = intentDetector;

		public async Task Handle(AICallCommand command, CancellationToken cancellationToken) {
			switch (command.CallSign) {
				case "AI" or "Bot" or "GPT" when command.ImageFileId is null && command.ReplyToMessage?.ImageFileId is null: {
						await _commandQueue.DispatchAsync(
							command: OpenAITextPrompt.FromAICallCommand(
								aiCallCommand: command,
								thread: command.ReplyToMessage is { } replyToMessage
									? _telegramMessageCache.GetThread(replyToMessage)
									: Enumerable.Empty<MessageBase>()
							)
						);
						break;
					}
				case "AI" or "Bot" or "GPT" when command.ImageFileId is not null || command.ReplyToMessage?.ImageFileId is not null: {
						await _commandQueue.DispatchAsync(
							command: OpenAIImagePrompt.FromAICallCommand(
								aiCallCommand: command,
								thread: command.ReplyToMessage is { } replyToMessage
									? _telegramMessageCache.GetThread(replyToMessage)
									: Enumerable.Empty<MessageBase>()
							)
						);
						break;
					}
			}
		}
	}
}
