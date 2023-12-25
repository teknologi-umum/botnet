using BotNet.Commands.Common;
using BotNet.Commands.Telegram;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Fuck {
	public sealed record FuckCommand : ICommand {
		public string Code { get; }
		public long ChatId { get; }
		public int CodeMessageId { get; }

		private FuckCommand(
			string code,
			long chatId,
			int codeMessageId
		) {
			Code = code;
			ChatId = chatId;
			CodeMessageId = codeMessageId;
		}

		public static FuckCommand FromSlashCommand(SlashCommand slashCommand) {
			if (slashCommand.Command != "/fuck") {
				throw new ArgumentException("Command must be /fuck.", nameof(slashCommand));
			}

			string code;
			int codeMessageId;

			// If replying to a message, then handle repliedToMessage 
			if (slashCommand.ReplyToMessage != null) {
				code = slashCommand.ReplyToMessage.Text;
				codeMessageId = slashCommand.ReplyToMessage.MessageId;
			} else {
				code = slashCommand.Text;
				codeMessageId = slashCommand.MessageId;
			}

			// Must have code
			if (string.IsNullOrWhiteSpace(code)) {
				throw new UsageException(
					message: "Kode tidak boleh kosong\\. Untuk menjalankan program brainfuck, silakan ketik `/fuck` diikuti kode program, atau reply `/fuck` ke pesan yang ada kodenya\\.",
					parseMode: ParseMode.MarkdownV2,
					commandMessageId: codeMessageId
				);
			}

			return new(
				code: code,
				chatId: slashCommand.ChatId,
				codeMessageId: codeMessageId
			);
		}
	}
}
