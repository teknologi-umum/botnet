using BotNet.Commands.Common;
using Telegram.Bot.Types.Enums;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;

namespace BotNet.Commands.Eval {
	public sealed record EvalCommand : ICommand {
		public string Command { get; }
		public string Code { get; }
		public MessageId CodeMessageId { get; }
		public ChatBase Chat { get; }

		private EvalCommand(
			string command,
			string code,
			MessageId codeMessageId,
			ChatBase chat
		) {
			Command = command;
			Code = code;
			CodeMessageId = codeMessageId;
			Chat = chat;
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
				codeMessageId: new(codeMessageId),
				chat: slashCommand.Chat
			);
		}
	}
}
