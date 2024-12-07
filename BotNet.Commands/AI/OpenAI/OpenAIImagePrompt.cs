using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands.AI.OpenAI {
	public sealed record OpenAiImagePrompt : ICommand {
		public string CallSign { get; }
		public string Prompt { get; }
		public string ImageFileId { get; }
		public HumanMessageBase Command { get; }
		public IEnumerable<MessageBase> Thread { get; }

		private OpenAiImagePrompt(
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

		public static OpenAiImagePrompt FromAiCallCommand(AiCallCommand aiCallCommand, IEnumerable<MessageBase> thread) {
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
			IEnumerable<MessageBase> messageBases = thread as MessageBase[] ?? thread.ToArray();
			if (!string.IsNullOrWhiteSpace(aiCallCommand.ImageFileId)) {
				imageFileId = aiCallCommand.ImageFileId;
			} else if (!string.IsNullOrWhiteSpace(messageBases.FirstOrDefault()?.ImageFileId)) {
				imageFileId = messageBases.First().ImageFileId!;
			} else {
				throw new ArgumentException("File ID must be non-empty.", nameof(aiCallCommand));
			}

			// Non-empty thread must begin with reply to message
			if (messageBases.FirstOrDefault() is not {
				    MessageId: var firstMessageId,
				    Chat.Id: var firstChatId
			    }) {
				return new(
					callSign: aiCallCommand.CallSign,
					prompt: aiCallCommand.Text,
					imageFileId: imageFileId,
					command: aiCallCommand,
					thread: messageBases
				);
			}

			if (firstMessageId != aiCallCommand.ReplyToMessage?.MessageId
			    || firstChatId != aiCallCommand.Chat.Id) {
				throw new ArgumentException("Thread must begin with reply to message.", nameof(thread));
			}

			return new OpenAiImagePrompt(
				callSign: aiCallCommand.CallSign,
				prompt: aiCallCommand.Text,
				imageFileId: imageFileId,
				command: aiCallCommand,
				thread: messageBases
			);
		}
	}
}
