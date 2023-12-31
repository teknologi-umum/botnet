using BotNet.Commands.CommandPrioritization;

namespace BotNet.Commands.AI.OpenAI {
	public sealed record class OpenAIImageGenerationPrompt : ICommand {
		public string CallSign { get; }
		public string Prompt { get; }
		public int PromptMessageId { get; }
		public int ResponseMessageId { get; }
		public long ChatId { get; }
		public long SenderId { get; }
		public CommandPriority CommandPriority { get; }

		public OpenAIImageGenerationPrompt(
			string callSign,
			string prompt,
			int promptMessageId,
			int responseMessageId,
			long chatId,
			long senderId,
			CommandPriority commandPriority
		) {
			if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentException($"'{nameof(prompt)}' cannot be null or whitespace.", nameof(prompt));

			CallSign = callSign;
			Prompt = prompt;
			PromptMessageId = promptMessageId;
			ResponseMessageId = responseMessageId;
			ChatId = chatId;
			SenderId = senderId;
			CommandPriority = commandPriority;
		}
	}
}
