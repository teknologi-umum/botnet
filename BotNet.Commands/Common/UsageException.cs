using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Common {
	public sealed class UsageException : Exception {
		public ParseMode ParseMode { get; }
		public int CommandMessageId { get; }

		public UsageException(
			string? message,
			ParseMode parseMode,
			int commandMessageId
		) : base(message) {
			ParseMode = parseMode;
			CommandMessageId = commandMessageId;
		}
	}
}
