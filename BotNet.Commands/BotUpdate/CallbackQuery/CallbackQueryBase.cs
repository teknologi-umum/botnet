namespace BotNet.Commands.BotUpdate.CallbackQuery {
	public abstract record CallbackQueryBase {
		public int MessageId { get; private set; }
		public long ChatId { get; private set; }
		public string? CallbackData { get; private set; }

		protected CallbackQueryBase(
			int messageId,
			long chatId,
			string? callbackData
		) {
			MessageId = messageId;
			ChatId = chatId;
			CallbackData = callbackData;
		}
	}
}
