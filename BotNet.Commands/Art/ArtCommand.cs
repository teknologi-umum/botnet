using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.CommandPrioritization;

namespace BotNet.Commands.Art {
	public sealed record ArtCommand : ICommand {
		public string Prompt { get; }
		public string? ImagePromptFileId { get; }
		public int PromptMessageId { get; }
		public long ChatId { get; }
		public long SenderId { get; }
		public CommandPriority CommandPriority { get; }
		public IEnumerable<MessageBase> Thread { get; }

		private ArtCommand(
			string prompt,
			string? imagePromptFileId,
			int promptMessageId,
			long chatId,
			long senderId,
			CommandPriority commandPriority,
			IEnumerable<MessageBase> thread
		) {
			Prompt = prompt;
			ImagePromptFileId = imagePromptFileId;
			PromptMessageId = promptMessageId;
			ChatId = chatId;
			SenderId = senderId;
			CommandPriority = commandPriority;
			Thread = thread;
		}

		public static ArtCommand FromSlashCommand(SlashCommand command, IEnumerable<MessageBase> thread) {
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
				imagePromptFileId: command.ImageFileId ?? command.ReplyToMessage?.ImageFileId,
				promptMessageId: command.MessageId,
				chatId: command.ChatId,
				senderId: command.SenderId,
				commandPriority: command.CommandPriority,
				thread: thread
			);
		}
	}
}
