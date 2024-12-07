using BotNet.Commands;
using BotNet.Commands.AI.Gemini;
using BotNet.Commands.AI.OpenAI;
using BotNet.Commands.BotUpdate.Message;

namespace BotNet.CommandHandlers.BotUpdate.Message {
	public sealed class AiFollowUpMessageHandler(
		ICommandQueue commandQueue,
		ITelegramMessageCache telegramMessageCache
	) : ICommandHandler<AiFollowUpMessage> {
		public async Task Handle(AiFollowUpMessage command, CancellationToken cancellationToken) {
			switch (command.CallSign) {
				// OpenAI GPT-4 Chat
				case "GPT":
					await commandQueue.DispatchAsync(
						command: OpenAiTextPrompt.FromAiFollowUpMessage(
							aiFollowUpMessage: command,
							thread: telegramMessageCache.GetThread(
								firstMessage: command.ReplyToMessage
							)
						)
					);
					break;

				// Google Gemini Chat
				case "AI" or "Bot" or "Gemini":
					await commandQueue.DispatchAsync(
						command: GeminiTextPrompt.FromAiFollowUpMessage(
							aIFollowUpMessage: command,
							thread: telegramMessageCache.GetThread(
								firstMessage: command.ReplyToMessage
							)
						)
					);
					break;
			}
		}
	}
}
