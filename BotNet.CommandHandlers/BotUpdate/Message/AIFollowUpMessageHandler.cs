using BotNet.Commands;
using BotNet.Commands.AI.Gemini;
using BotNet.Commands.AI.OpenAI;
using BotNet.Commands.BotUpdate.Message;

namespace BotNet.CommandHandlers.BotUpdate.Message {
	public sealed class AIFollowUpMessageHandler(
		ICommandQueue commandQueue,
		ITelegramMessageCache telegramMessageCache
	) : ICommandHandler<AIFollowUpMessage> {
		private readonly ICommandQueue _commandQueue = commandQueue;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;

		public async Task Handle(AIFollowUpMessage command, CancellationToken cancellationToken) {
			switch (command.CallSign) {
				// OpenAI GPT-4 Chat
				case "GPT":
					await _commandQueue.DispatchAsync(
						command: OpenAITextPrompt.FromAIFollowUpMessage(
							aiFollowUpMessage: command,
							thread: command.ReplyToMessage is null
								? Enumerable.Empty<MessageBase>()
								: _telegramMessageCache.GetThread(
									firstMessage: command.ReplyToMessage
								)
						)
					);
					break;

				// Google Gemini Chat
				case "AI" or "Bot" or "Gemini":
					await _commandQueue.DispatchAsync(
						command: GeminiTextPrompt.FromAIFollowUpMessage(
							aIFollowUpMessage: command,
							thread: command.ReplyToMessage is null
								? Enumerable.Empty<MessageBase>()
								: _telegramMessageCache.GetThread(
									firstMessage: command.ReplyToMessage
								)
						)
					);
					break;
			}
		}
	}
}
