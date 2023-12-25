using BotNet.Commands.Telegram;

namespace BotNet.Commands {
	public interface ITelegramMessageCache {
		void Set(MessageBase message);
		MessageBase? GetOrDefault(int messageId, long chatId);
		IEnumerable<MessageBase> GetThread(int messageId, long chatId);
	}
}
