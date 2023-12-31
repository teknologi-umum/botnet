using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.Common;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.GoogleMaps {
	public sealed record MapCommand : ICommand {
		public string PlaceName { get; }
		public int CommandMessageId { get; }
		public long ChatId { get; }
		public long SenderId { get; }

		private MapCommand(
			string placeName,
			int commandMessageId,
			long chatId,
			long senderId
		) {
			PlaceName = placeName;
			CommandMessageId = commandMessageId;
			ChatId = chatId;
			SenderId = senderId;
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
				chatId: slashCommand.ChatId,
				senderId: slashCommand.SenderId
			);
		}
	}
}
