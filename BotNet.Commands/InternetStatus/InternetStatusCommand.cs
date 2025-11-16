using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.SenderAggregate;

namespace BotNet.Commands.InternetStatus {
	public sealed record InternetStatusCommand : ICommand {
		public ChatBase Chat { get; }
		public MessageId MessageId { get; }
		public SenderBase Sender { get; }

		private InternetStatusCommand(
			ChatBase chat,
			MessageId messageId,
			SenderBase sender
		) {
			Chat = chat;
			MessageId = messageId;
			Sender = sender;
		}

		public static InternetStatusCommand FromSlashCommand(SlashCommand slashCommand) {
			return new InternetStatusCommand(
				chat: slashCommand.Chat,
				messageId: slashCommand.MessageId,
				sender: slashCommand.Sender
			);
		}
	}
}
