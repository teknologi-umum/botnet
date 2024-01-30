using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;

namespace BotNet.Commands {
	public interface ITelegramMessageCache {
		void Add(MessageBase message);
		MessageBase? GetOrDefault(MessageId messageId, ChatId chatId);
		IEnumerable<MessageBase> GetThread(MessageId messageId, ChatId chatId);
		IEnumerable<MessageBase> GetThread(MessageBase firstMessage);
	}
}
