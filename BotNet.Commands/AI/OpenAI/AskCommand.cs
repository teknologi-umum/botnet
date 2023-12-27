using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands.AI.OpenAI {
	public sealed record AskCommand : ICommand {
		public string Prompt { get; }
		public int PromptMessageId { get; }
		public long ChatId { get; }
		public long SenderId { get; }
		public IEnumerable<MessageBase> Thread { get; }

		private AskCommand(
			string prompt,
			int promptMessageId,
			long chatId,
			long senderId,
			IEnumerable<MessageBase> thread
		) {
			Prompt = prompt;
			PromptMessageId = promptMessageId;
			ChatId = chatId;
			SenderId = senderId;
			Thread = thread;
		}

		public static AskCommand FromSlashCommand(SlashCommand command, IEnumerable<MessageBase> thread) {
			// Prompt must be non-empty
			if (string.IsNullOrWhiteSpace(command.Text)) {
				throw new ArgumentException("Prompt must be non-empty.", nameof(command));
			}

			// Non-empty thread must begin with reply to message
			if (thread.FirstOrDefault() is {
				MessageId: { } firstMessageId,
				ChatId: { } firstChatId
			}) {
				if (firstMessageId != command.ReplyToMessageId
					|| firstChatId != command.ChatId) {
					throw new ArgumentException("Thread must begin with reply to message.", nameof(thread));
				}
			}

			return new(
				prompt: command.Text,
				promptMessageId: command.MessageId,
				chatId: command.ChatId,
				senderId: command.SenderId,
				thread: thread
			);
		}
	}
}
