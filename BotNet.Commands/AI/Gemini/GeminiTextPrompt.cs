using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands.AI.Gemini {
	public sealed record GeminiTextPrompt : ICommand {
		public string Prompt { get; }
		public HumanMessageBase Command { get; }
		public IEnumerable<MessageBase> Thread { get; }

		private GeminiTextPrompt(
			string prompt,
			HumanMessageBase command,
			IEnumerable<MessageBase> thread
		) {
			Prompt = prompt;
			Command = command;
			Thread = thread;
		}

		public static GeminiTextPrompt FromAiCallCommand(AiCallCommand aiCallCommand, IEnumerable<MessageBase> thread) {
			// Call sign must be Gemini, AI, or Bot
			if (aiCallCommand.CallSign is not "Gemini" and not "AI" and not "Bot") {
				throw new ArgumentException("Call sign must be Gemini, AI, or Bot", nameof(aiCallCommand));
			}

			// Prompt must be non-empty
			if (string.IsNullOrWhiteSpace(aiCallCommand.Text)) {
				throw new ArgumentException("Prompt must be non-empty", nameof(aiCallCommand));
			}

			// Non-empty thread must begin with reply to message
			IEnumerable<MessageBase> messageBases = thread as MessageBase[] ?? thread.ToArray();
			if (messageBases.FirstOrDefault() is not {
				    MessageId: var firstMessageId,
				    Chat.Id: var firstChatId
			    }) {
				return new GeminiTextPrompt(
					prompt: aiCallCommand.Text,
					command: aiCallCommand,
					thread: messageBases
				);
			}

			if (firstMessageId != aiCallCommand.ReplyToMessage?.MessageId
			    || firstChatId != aiCallCommand.Chat.Id) {
				throw new ArgumentException("Thread must begin with reply to message", nameof(thread));
			}

			return new GeminiTextPrompt(
				prompt: aiCallCommand.Text,
				command: aiCallCommand,
				thread: messageBases
			);
		}

		public static GeminiTextPrompt FromAiFollowUpMessage(AiFollowUpMessage aIFollowUpMessage, IEnumerable<MessageBase> thread) {
			// Call sign must be Gemini, AI, or Bot
			if (aIFollowUpMessage.CallSign is not "Gemini" and not "AI" and not "Bot") {
				throw new ArgumentException("Call sign must be Gemini, AI, or Bot", nameof(aIFollowUpMessage));
			}

			// Prompt must be non-empty
			if (string.IsNullOrWhiteSpace(aIFollowUpMessage.Text)) {
				throw new ArgumentException("Prompt must be non-empty", nameof(aIFollowUpMessage));
			}

			// Non-empty thread must begin with reply to message
			IEnumerable<MessageBase> messageBases = thread as MessageBase[] ?? thread.ToArray();
			if (messageBases.FirstOrDefault() is not {
				    MessageId: var firstMessageId,
				    Chat.Id: var firstChatId
			    }) {
				return new GeminiTextPrompt(
					prompt: aIFollowUpMessage.Text,
					command: aIFollowUpMessage,
					thread: messageBases
				);
			}

			if (firstMessageId != aIFollowUpMessage.ReplyToMessage.MessageId
			    || firstChatId != aIFollowUpMessage.Chat.Id) {
				throw new ArgumentException("Thread must begin with reply to message", nameof(thread));
			}

			return new GeminiTextPrompt(
				prompt: aIFollowUpMessage.Text,
				command: aIFollowUpMessage,
				thread: messageBases
			);
		}
	}
}
