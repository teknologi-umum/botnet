using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands {
	public interface ITelegramMessageCache {
		void Add(MessageBase message);
		MessageBase? GetOrDefault(int messageId, long chatId);
		IEnumerable<MessageBase> GetThread(int messageId, long chatId);
	}
}
