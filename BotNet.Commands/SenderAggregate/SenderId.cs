namespace BotNet.Commands.SenderAggregate {
	public readonly record struct SenderId(long Value) {
		public static implicit operator long(SenderId senderId) => senderId.Value;
		public static implicit operator SenderId(long senderId) => new(senderId);
	}
}
