using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.SenderAggregate;

namespace BotNet.Commands.BMKG {
	public sealed record BMKGCommand : ICommand {
		public MessageId CommandMessageId { get; }
		public ChatBase Chat { get; }
		public HumanSender Sender { get; }

		private BMKGCommand(
			MessageId commandMessageId,
			ChatBase chat,
			HumanSender sender
		) {
			CommandMessageId = commandMessageId;
			Chat = chat;
			Sender = sender;
		}

		public static BMKGCommand FromSlashCommand(SlashCommand slashCommand) {
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
