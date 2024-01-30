using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands.AI.OpenAI {
	public sealed record AskCommand : ICommand {
		public string Prompt { get; }
		public SlashCommand Command { get; }
		public IEnumerable<MessageBase> Thread { get; }

		private AskCommand(
			string prompt,
			SlashCommand command,
			IEnumerable<MessageBase> thread
		) {
			Prompt = prompt;
			Command = command;
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
				Chat.Id: { } firstChatId
			}) {
				if (firstMessageId != command.ReplyToMessage?.MessageId
					|| firstChatId != command.Chat.Id) {
					throw new ArgumentException("Thread must begin with reply to message.", nameof(thread));
				}
			}

			return new(
				prompt: command.Text,
				command: command,
				thread: thread
			);
		}
	}
}
