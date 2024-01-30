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
			// Call sign must be AI, Bot, or GPT
			if (aiCallCommand.CallSign is not "AI" and not "Bot" and not "GPT") {
				throw new ArgumentException("Call sign must be AI, Bot, or GPT.", nameof(aiCallCommand));
			}

			// Prompt must be non-empty
			if (string.IsNullOrWhiteSpace(aiCallCommand.Text)) {
				throw new ArgumentException("Prompt must be non-empty.", nameof(aiCallCommand));
			}

			// File ID must be non-empty
			if (string.IsNullOrWhiteSpace(aiCallCommand.ImageFileId)) {
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
				imageFileId: aiCallCommand.ImageFileId,
				command: aiCallCommand,
				thread: thread
			);
		}
	}
}
