using BotNet.Commands.Common;
using Telegram.Bot.Types.Enums;
using BotNet.Commands.BotUpdate.Message;

namespace BotNet.Commands.Eval {
	public sealed record EvalCommand : ICommand {
		public string Command { get; }
		public string Code { get; }
		public long ChatId { get; }
		public int CodeMessageId { get; }

		private EvalCommand(
			string command,
			string code,
			long chatId,
			int codeMessageId
		) {
			Command = command;
			Code = code;
			ChatId = chatId;
			CodeMessageId = codeMessageId;
		}

		public static EvalCommand FromSlashCommand(SlashCommand slashCommand) {
			string language = slashCommand.Command switch {
				"/evaljs" => "javascript",
				"/evalcs" => "C\\#", // C#, but escaped for Markdown
				_ => throw new ArgumentException("Command must be /evaljs or /evalcs.", nameof(slashCommand))
			};

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
					message: $"Kode tidak boleh kosong\\. Untuk mengevaluasi {language}, silakan ketik `{slashCommand.Command}` diikuti ekspresi {language}, atau reply `{slashCommand.Command}` ke pesan yang ada kodenya\\.",
					parseMode: ParseMode.MarkdownV2,
					commandMessageId: codeMessageId
				);
			}

			return new(
				command: slashCommand.Command,
				code: code,
				chatId: slashCommand.ChatId,
				codeMessageId: codeMessageId
			);
		}
	}
}
