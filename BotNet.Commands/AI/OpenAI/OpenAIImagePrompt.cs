using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands.AI.OpenAI {
	public sealed record OpenAIImagePrompt : ICommand {
		public string CallSign { get; }
		public string Prompt { get; }
		public string ImageFileId { get; }
		public HumanMessageBase Command { get; }
		public IEnumerable<MessageBase> Thread { get; }

		private OpenAIImagePrompt(
			string callSign,
			string prompt,
			string imageFileId,
			HumanMessageBase command,
			IEnumerable<MessageBase> thread
		) {
			CallSign = callSign;
			Prompt = prompt;
			ImageFileId = imageFileId;
			Command = command;
			Thread = thread;
		}

		public static OpenAIImagePrompt FromAICallCommand(AICallCommand aiCallCommand, IEnumerable<MessageBase> thread) {
			// Call sign must be GPT
			if (aiCallCommand.CallSign is not "GPT") {
				throw new ArgumentException("Call sign must be GPT.", nameof(aiCallCommand));
			}

			// Prompt must be non-empty
			if (string.IsNullOrWhiteSpace(aiCallCommand.Text)) {
				throw new ArgumentException("Prompt must be non-empty.", nameof(aiCallCommand));
			}

			// File ID must be non-empty
			string imageFileId;
			if (!string.IsNullOrWhiteSpace(aiCallCommand.ImageFileId)) {
				imageFileId = aiCallCommand.ImageFileId;
			} else if (!string.IsNullOrWhiteSpace(thread.FirstOrDefault()?.ImageFileId)) {
				imageFileId = thread.First().ImageFileId!;
			} else {
				throw new ArgumentException("File ID must be non-empty.", nameof(aiCallCommand));
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
				imageFileId: imageFileId,
				command: aiCallCommand,
				thread: thread
			);
		}
	}
}
