namespace BotNet.Commands.AI.Stability {
	public sealed record StabilityTextToImagePrompt : ICommand {
		public string CallSign { get; }
		public string Prompt { get; }
		public int PromptMessageId { get; }
		public int ResponseMessageId { get; }
		public long ChatId { get; }
		public long SenderId { get; }

		public StabilityTextToImagePrompt(
			string callSign,
			string prompt,
			int promptMessageId,
			int responseMessageId,
			long chatId,
			long senderId
		) {
			if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentException($"'{nameof(prompt)}' cannot be null or whitespace.", nameof(prompt));

			CallSign = callSign;
			Prompt = prompt;
			PromptMessageId = promptMessageId;
			ResponseMessageId = responseMessageId;
			ChatId = chatId;
			SenderId = senderId;
		}
	}
}
