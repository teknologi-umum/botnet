using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.SenderAggregate;

namespace BotNet.Commands.Privacy {
	public sealed record PrivacyCommand : ICommand {
		public MessageId CommandMessageId { get; }
		public ChatBase Chat { get; }
		public HumanSender Sender { get; }

		private PrivacyCommand(
			MessageId commandMessageId,
			ChatBase chat,
			HumanSender sender
		) {
			CommandMessageId = commandMessageId;
			Chat = chat;
			Sender = sender;
		}

		public static PrivacyCommand FromSlashCommand(SlashCommand command) {
			return new(
				commandMessageId: command.MessageId,
				chat: command.Chat,
				sender: command.Sender
			);
		}
	}
}
