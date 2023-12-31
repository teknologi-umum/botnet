using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.CommandPrioritization;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Privilege {
	public sealed record PrivilegeCommand : ICommand {
		public int CommandMessageId { get; }
		public long ChatId { get; }
		public ChatType ChatType { get; }
		public string? ChatTitle { get; }
		public long SenderId { get; }
		public string SenderName { get; }
		public CommandPriority CommandPriority { get; }

		private PrivilegeCommand(
			int commandMessageId,
			long chatId,
			ChatType chatType,
			string? chatTitle,
			long senderId,
			string senderName,
			CommandPriority commandPriority
		) {
			CommandMessageId = commandMessageId;
			ChatId = chatId;
			ChatType = chatType;
			ChatTitle = chatTitle;
			SenderId = senderId;
			SenderName = senderName;
			CommandPriority = commandPriority;
		}

		public static PrivilegeCommand FromSlashCommand(SlashCommand command) {
			return new(
				commandMessageId: command.MessageId,
				chatId: command.ChatId,
				chatType: command.ChatType,
				chatTitle: command.ChatTitle,
				senderId: command.SenderId,
				senderName: command.SenderName,
				commandPriority: command.CommandPriority
			);
		}
	}
}
