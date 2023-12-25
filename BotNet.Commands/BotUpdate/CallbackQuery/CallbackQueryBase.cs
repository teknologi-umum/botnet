using BotNet.Commands.Pop;

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

		public static CallbackQueryBase FromCallbackQuery(Telegram.Bot.Types.CallbackQuery callbackQuery) {
			// Callback query must contain data
			if (callbackQuery.Data is not { } data) {
				throw new ArgumentException("Callback query must contain data.", nameof(callbackQuery));
			}

			// Handle bubble wrap callback
			if (data.StartsWith("pop:")) {
				return BubbleWrapCallback.FromCallbackQuery(callbackQuery);
			}

			throw new ArgumentException("Unrecognized callback data.", nameof(callbackQuery));
		}
	}
}
