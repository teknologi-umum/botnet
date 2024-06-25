using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;

namespace BotNet.Commands.Khodam {
	public sealed record KhodamCommand : ICommand {
		public MessageId TargetMessageId { get; }
		public ChatBase Chat { get; }
		public string Name { get; }
		public long UserId { get; }

		private KhodamCommand(
			MessageId targetMessageId,
			ChatBase chat,
			string name,
			long userId
		) {
			TargetMessageId = targetMessageId;
			Chat = chat;
			Name = name;
			UserId = userId;
		}

		public static KhodamCommand FromSlashCommand(SlashCommand slashCommand) {
			if (slashCommand.Command != "/khodam") {
				throw new ArgumentException("Command must be /khodam.", nameof(slashCommand));
			}

			return new(
				targetMessageId: slashCommand.MessageId,
				chat: slashCommand.Chat,
				name: slashCommand.ReplyToMessage?.Sender.Name ?? slashCommand.Sender.Name,
				userId: slashCommand.ReplyToMessage?.Sender.Id ?? slashCommand.Sender.Id
			);
		}
	}
}
