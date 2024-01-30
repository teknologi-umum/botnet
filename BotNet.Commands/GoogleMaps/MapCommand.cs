using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Common;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.GoogleMaps {
	public sealed record MapCommand : ICommand {
		public string PlaceName { get; }
		public MessageId CommandMessageId { get; }
		public ChatBase Chat { get; }
		public HumanSender Sender { get; }

		private MapCommand(
			string placeName,
			MessageId commandMessageId,
			ChatBase chat,
			HumanSender sender
		) {
			PlaceName = placeName;
			CommandMessageId = commandMessageId;
			Chat = chat;
			Sender = sender;
		}

		public static MapCommand FromSlashCommand(SlashCommand slashCommand) {
			// Must be /map
			if (slashCommand.Command != "/map") {
				throw new ArgumentException("Command must be /map.", nameof(slashCommand));
			}

			// Place name must be non-empty
			if (string.IsNullOrWhiteSpace(slashCommand.Text)) {
				throw new UsageException(
					message: "Silakan masukkan nama lokasi setelah perintah `/map`\\.",
					parseMode: ParseMode.MarkdownV2,
					commandMessageId: slashCommand.MessageId
				);
			}

			return new(
				placeName: slashCommand.Text,
				commandMessageId: slashCommand.MessageId,
				chat: slashCommand.Chat,
				sender: slashCommand.Sender
			);
		}
	}
}
