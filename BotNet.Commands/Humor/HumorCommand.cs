using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.SenderAggregate;

namespace BotNet.Commands.Humor {
	public sealed record HumorCommand : ICommand {
		public MessageId CommandMessageId { get; }
		public ChatBase Chat { get; }
		public HumanSender Sender { get; }

		private HumorCommand(
			MessageId commandMessageId,
			ChatBase chat,
			HumanSender sender
		) {
			CommandMessageId = commandMessageId;
			Chat = chat;
			Sender = sender;
		}

		public static HumorCommand FromSlashCommand(SlashCommand slashCommand) {
			if (slashCommand.Command != "/humor") {
				throw new ArgumentException("Command must be /humor.", nameof(slashCommand));
			}

			return new(
				commandMessageId: slashCommand.MessageId,
				chat: slashCommand.Chat,
				sender: slashCommand.Sender
			);
		}
	}
}
