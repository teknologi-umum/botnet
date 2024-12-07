using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.SenderAggregate;

namespace BotNet.Commands.BMKG {
	public sealed record BmkgCommand : ICommand {
		public MessageId CommandMessageId { get; }
		public ChatBase Chat { get; }
		public HumanSender Sender { get; }

		private BmkgCommand(
			MessageId commandMessageId,
			ChatBase chat,
			HumanSender sender
		) {
			CommandMessageId = commandMessageId;
			Chat = chat;
			Sender = sender;
		}

		public static BmkgCommand FromSlashCommand(SlashCommand slashCommand) {
			// Must be /bmkg
			if (slashCommand.Command != "/bmkg") {
				throw new ArgumentException("Command must be /bmkg.", nameof(slashCommand));
			}

			return new(
				commandMessageId: slashCommand.MessageId,
				chat: slashCommand.Chat,
				sender: slashCommand.Sender
			);
		}
	}
}
