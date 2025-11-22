using BotNet.Commands;
using BotNet.Commands.AI.Gemini;
using Mediator;
using BotNet.Commands.AI.OpenAI;
using BotNet.Commands.BotUpdate.Message;

namespace BotNet.CommandHandlers.BotUpdate.Message {
	public sealed class AiCallCommandHandler(
		ICommandQueue commandQueue,
		ITelegramMessageCache telegramMessageCache
	) : ICommandHandler<AiCallCommand> {
		public async ValueTask<Unit> Handle(AiCallCommand command, CancellationToken cancellationToken) {
			switch (command.CallSign) {
				case "GPT" when command.ImageFileId is null && command.ReplyToMessage?.ImageFileId is null: {
						await commandQueue.DispatchAsync(
							command: OpenAiTextPrompt.FromAiCallCommand(
								aiCallCommand: command,
								thread: command.ReplyToMessage is { } replyToMessage
									? telegramMessageCache.GetThread(replyToMessage)
									: []
							)
						);
						break;
		return default;
					}
				case "GPT" when command.ImageFileId is not null || command.ReplyToMessage?.ImageFileId is not null: {
						await commandQueue.DispatchAsync(
							command: OpenAiImagePrompt.FromAiCallCommand(
								aiCallCommand: command,
								thread: command.ReplyToMessage is { } replyToMessage
									? telegramMessageCache.GetThread(replyToMessage)
									: []
							)
						);
						break;
					}
				case "AI" or "Bot" or "Gemini" when command.ImageFileId is null && command.ReplyToMessage?.ImageFileId is null: {
						await commandQueue.DispatchAsync(
							command: GeminiTextPrompt.FromAiCallCommand(
								aiCallCommand: command,
								thread: command.ReplyToMessage is { } replyToMessage
									? telegramMessageCache.GetThread(replyToMessage)
									: []
							)
						);
						break;
					}
			}
	return default;
		}
	}
}
