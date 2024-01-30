using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands.AI.OpenAI {
	public sealed record OpenAITextPrompt : ICommand {
		public string CallSign { get; }
		public string Prompt { get; }
		public HumanMessageBase Command { get; }
		public IEnumerable<MessageBase> Thread { get; }

		private OpenAITextPrompt(
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

		public static OpenAITextPrompt FromAICallCommand(AICallCommand aiCallCommand, IEnumerable<MessageBase> thread) {
			// Call sign must be AI, Bot, or GPT
			if (aiCallCommand.CallSign is not "AI" and not "Bot" and not "GPT") {
				throw new ArgumentException("Call sign must be AI, Bot, or GPT.", nameof(aiCallCommand));
			}

			// Prompt must be non-empty
			if (string.IsNullOrWhiteSpace(aiCallCommand.Text)) {
				throw new ArgumentException("Prompt must be non-empty.", nameof(aiCallCommand));
			}

			// Non-empty thread must begin with reply to message
			if (thread.FirstOrDefault() is {
				MessageId: { } firstMessageId,
				Chat.Id: { } firstChatId
			}) {
				if (firstMessageId != aiCallCommand.ReplyToMessage?.MessageId
					|| firstChatId != aiCallCommand.Chat.Id) {
					throw new ArgumentException("Thread must begin with reply to message.", nameof(thread));
				}
			}

			return new(
				callSign: aiCallCommand.CallSign,
				prompt: aiCallCommand.Text,
				command: aiCallCommand,
				thread: thread
			);
		}

		public static OpenAITextPrompt FromAIFollowUpMessage(AIFollowUpMessage aiFollowUpMessage, IEnumerable<MessageBase> thread) {
			// Call sign must be AI, Bot, or GPT
			if (aiFollowUpMessage.CallSign is not "AI" and not "Bot" and not "GPT") {
				throw new ArgumentException("Call sign must be AI, Bot, or GPT.", nameof(aiFollowUpMessage));
			}

			// Prompt must be non-empty
			if (string.IsNullOrWhiteSpace(aiFollowUpMessage.Text)) {
				throw new ArgumentException("Prompt must be non-empty.", nameof(aiFollowUpMessage));
			}

			// Non-empty thread must begin with reply to message
			if (thread.FirstOrDefault() is {
				MessageId: { } firstMessageId,
				Chat.Id: { } firstChatId
			}) {
				if (firstMessageId != aiFollowUpMessage.ReplyToMessage.MessageId
					|| firstChatId != aiFollowUpMessage.Chat.Id) {
					throw new ArgumentException("Thread must begin with reply to message.", nameof(thread));
				}
			}

			return new(
				callSign: aiFollowUpMessage.CallSign,
				prompt: aiFollowUpMessage.Text,
				command: aiFollowUpMessage,
				thread: thread
			);
		}
	}
}
