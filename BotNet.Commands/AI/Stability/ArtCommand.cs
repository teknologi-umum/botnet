using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.CommandPrioritization;
using BotNet.Commands.Common;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.AI.Stability {
	public sealed record ArtCommand : ICommand {
		public string Prompt { get; }
		public int PromptMessageId { get; }
		public long ChatId { get; }
		public long SenderId { get; }
		public CommandPriority CommandPriority { get; }

		private ArtCommand(
			string prompt,
			int promptMessageId,
			long chatId,
			long senderId,
			CommandPriority commandPriority
		) {
			Prompt = prompt;
			PromptMessageId = promptMessageId;
			ChatId = chatId;
			SenderId = senderId;
			CommandPriority = commandPriority;
		}

		public static ArtCommand FromSlashCommand(SlashCommand command) {
			// Prompt must be non-empty
			if (string.IsNullOrWhiteSpace(command.Text)) {
				throw new UsageException(
					message: "Prompt tidak boleh kosong\\. Untuk menghasilkan gambar, silakan ketik `/art` diikuti prompt\\.",
					parseMode: ParseMode.MarkdownV2,
					commandMessageId: command.MessageId
				);
			}

			return new(
				prompt: command.Text,
				promptMessageId: command.MessageId,
				chatId: command.ChatId,
				senderId: command.SenderId,
				commandPriority: command.CommandPriority
			);
		}
	}
}
