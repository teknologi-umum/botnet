namespace BotNet.Commands.ChatAggregate {
	public readonly record struct ChatId(long Value) {
		public static implicit operator long(ChatId chatId) => chatId.Value;

		public static implicit operator ChatId(Telegram.Bot.Types.ChatId chatId) {
			if (chatId.Identifier is null) {
				throw new ArgumentException("ChatId.Identifier is null");
			}
			return new(chatId.Identifier.Value);
		}

		public static implicit operator Telegram.Bot.Types.ChatId(ChatId chatId) => new(chatId.Value);
	}
}
