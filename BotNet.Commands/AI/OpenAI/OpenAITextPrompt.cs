using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands.AI.OpenAI {
	public sealed record OpenAiTextPrompt : ICommand {
		public string CallSign { get; }
		public string Prompt { get; }
		public HumanMessageBase Command { get; }
		public IEnumerable<MessageBase> Thread { get; }

		private OpenAiTextPrompt(
			string callSign,
			string prompt,
			HumanMessageBase command,
			IEnumerable<MessageBase> thread
		) {
			CallSign = callSign;
			Prompt = prompt;
			Command = command;
			Thread = thread;
		}

		public static OpenAiTextPrompt FromAiCallCommand(AiCallCommand aiCallCommand, IEnumerable<MessageBase> thread) {
			// Call sign must be GPT
			if (aiCallCommand.CallSign is not "GPT") {
				throw new ArgumentException("Call sign must be GPT.", nameof(aiCallCommand));
			}

			// Prompt must be non-empty
			if (string.IsNullOrWhiteSpace(aiCallCommand.Text)) {
				throw new ArgumentException("Prompt must be non-empty.", nameof(aiCallCommand));
			}

			// Non-empty thread must begin with reply to message
			IEnumerable<MessageBase> messageBases = thread as MessageBase[] ?? thread.ToArray();
			if (messageBases.FirstOrDefault() is not {
				    MessageId: var firstMessageId,
				    Chat.Id: var firstChatId
			    }) {
				return new(
					callSign: aiCallCommand.CallSign,
					prompt: aiCallCommand.Text,
					command: aiCallCommand,
					thread: messageBases
				);
			}

			if (firstMessageId != aiCallCommand.ReplyToMessage?.MessageId
			    || firstChatId != aiCallCommand.Chat.Id) {
				throw new ArgumentException("Thread must begin with reply to message.", nameof(thread));
			}

			return new(
				callSign: aiCallCommand.CallSign,
				prompt: aiCallCommand.Text,
				command: aiCallCommand,
				thread: messageBases
			);
		}

		public static OpenAiTextPrompt FromAiFollowUpMessage(AiFollowUpMessage aiFollowUpMessage, IEnumerable<MessageBase> thread) {
			// Call sign must be GPT
			if (aiFollowUpMessage.CallSign is not "GPT") {
				throw new ArgumentException("Call sign must be GPT.", nameof(aiFollowUpMessage));
			}

			// Prompt must be non-empty
			if (string.IsNullOrWhiteSpace(aiFollowUpMessage.Text)) {
				throw new ArgumentException("Prompt must be non-empty.", nameof(aiFollowUpMessage));
			}

			// Non-empty thread must begin with reply to message
			IEnumerable<MessageBase> messageBases = thread as MessageBase[] ?? thread.ToArray();
			if (messageBases.FirstOrDefault() is not {
				    MessageId: var firstMessageId,
				    Chat.Id: var firstChatId
			    }) {
				return new(
					callSign: aiFollowUpMessage.CallSign,
					prompt: aiFollowUpMessage.Text,
					command: aiFollowUpMessage,
					thread: messageBases
				);
			}

			if (firstMessageId != aiFollowUpMessage.ReplyToMessage.MessageId
			    || firstChatId != aiFollowUpMessage.Chat.Id) {
				throw new ArgumentException("Thread must begin with reply to message.", nameof(thread));
			}

			return new(
				callSign: aiFollowUpMessage.CallSign,
				prompt: aiFollowUpMessage.Text,
				command: aiFollowUpMessage,
				thread: messageBases
			);
		}
	}
}
