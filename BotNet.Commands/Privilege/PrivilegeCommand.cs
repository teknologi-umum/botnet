using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.SenderAggregate;

namespace BotNet.Commands.Privilege {
	public sealed record PrivilegeCommand : ICommand {
		public MessageId CommandMessageId { get; }
		public ChatBase Chat { get; }
		public HumanSender Sender { get; }

		private PrivilegeCommand(
			MessageId commandMessageId,
			ChatBase chat,
			HumanSender sender
		) {
			CommandMessageId = commandMessageId;
			Chat = chat;
			Sender = sender;
		}

		public static PrivilegeCommand FromSlashCommand(SlashCommand command) {
			return new(
				commandMessageId: command.MessageId,
				chat: command.Chat,
				sender: command.Sender
			);
		}
	}
}
