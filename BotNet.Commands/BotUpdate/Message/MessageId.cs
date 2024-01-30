namespace BotNet.Commands.BotUpdate.Message {
	public readonly record struct MessageId(int Value) {
		public static implicit operator int(MessageId messageId) => messageId.Value;
		public static implicit operator MessageId(Telegram.Bot.Types.MessageId messageId) => new(messageId.Id);
	}
}
