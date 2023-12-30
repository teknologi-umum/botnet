using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.CommandPrioritization;

namespace BotNet.Commands.AI.OpenAI {
	public sealed record OpenAITextPrompt : ICommand {
		public string CallSign { get; }
		public string Prompt { get; }
		public int PromptMessageId { get; }
		public long ChatId { get; }
		public long SenderId { get; }
		public CommandPriority CommandPriority { get; }
		public IEnumerable<MessageBase> Thread { get; }

		private OpenAITextPrompt(
			string callSign,
			string prompt,
			int promptMessageId,
			long chatId,
			long senderId,
			CommandPriority commandPriority,
			IEnumerable<MessageBase> thread
		) {
			CallSign = callSign;
			Prompt = prompt;
			PromptMessageId = promptMessageId;
			ChatId = chatId;
			SenderId = senderId;
			CommandPriority = commandPriority;
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
				ChatId: { } firstChatId
			}) {
				if (firstMessageId != aiCallCommand.ReplyToMessageId
					|| firstChatId != aiCallCommand.ChatId) {
					throw new ArgumentException("Thread must begin with reply to message.", nameof(thread));
				}
			}

			return new(
				callSign: aiCallCommand.CallSign,
				prompt: aiCallCommand.Text,
				promptMessageId: aiCallCommand.MessageId,
				chatId: aiCallCommand.ChatId,
				senderId: aiCallCommand.SenderId,
				commandPriority: aiCallCommand.CommandPriority,
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
				ChatId: { } firstChatId
			}) {
				if (firstMessageId != aiFollowUpMessage.ReplyToMessageId
					|| firstChatId != aiFollowUpMessage.ChatId) {
					throw new ArgumentException("Thread must begin with reply to message.", nameof(thread));
				}
			}

			return new(
				callSign: aiFollowUpMessage.CallSign,
				prompt: aiFollowUpMessage.Text,
				promptMessageId: aiFollowUpMessage.MessageId,
				chatId: aiFollowUpMessage.ChatId,
				senderId: aiFollowUpMessage.SenderId,
				commandPriority: aiFollowUpMessage.CommandPriority,
				thread: thread
			);
		}
	}
}
