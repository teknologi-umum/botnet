using BotNet.Commands.Common;
using Telegram.Bot.Types.Enums;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;

namespace BotNet.Commands.Exec {
	public sealed record ExecCommand : ICommand {
		public string PistonLanguageIdentifier { get; }
		public string HighlightLanguageIdentifier { get; }
		public string Code { get; }
		public bool IsMentioned { get; }
		public MessageId CodeMessageId { get; }
		public ChatBase Chat { get; }

		private ExecCommand(
			string pistonLanguageIdentifier,
			string highlightLanguageIdentifier,
			string code,
			bool isMentioned,
			MessageId codeMessageId,
			ChatBase chat
		) {
			PistonLanguageIdentifier = pistonLanguageIdentifier;
			HighlightLanguageIdentifier = highlightLanguageIdentifier;
			Code = code;
			IsMentioned = isMentioned;
			CodeMessageId = codeMessageId;
			Chat = chat;
		}

		public static ExecCommand FromSlashCommand(SlashCommand slashCommand) {
			string pistonLanguageIdentifier;
			string highlightLanguageIdentifier;

			switch (slashCommand.Command) {
				case "/c":
				case "/clojure":
				case "/crystal":
				case "/dart":
				case "/elixir":
				case "/go":
				case "/java":
				case "/kotlin":
				case "/lua":
				case "/pascal":
				case "/php":
				case "/python":
				case "/ruby":
				case "/rust":
				case "/scala":
				case "/swift":
				case "/julia":
				case "/sqlite3":
					pistonLanguageIdentifier = slashCommand.Command[1..];
					highlightLanguageIdentifier = pistonLanguageIdentifier;
					break;
				case "/commonlisp":
					pistonLanguageIdentifier = "commonlisp";
					highlightLanguageIdentifier = "cl";
					break;
				case "/cpp":
					pistonLanguageIdentifier = "c++";
					highlightLanguageIdentifier = "cpp";
					break;
				case "/cs":
					pistonLanguageIdentifier = "csharp.net";
					highlightLanguageIdentifier = "csharp";
					break;
				case "/fs":
					pistonLanguageIdentifier = "fsharp.net";
					highlightLanguageIdentifier = "fsharp";
					break;
				case "/js":
					pistonLanguageIdentifier = "javascript";
					highlightLanguageIdentifier = "js";
					break;
				case "/ts":
					pistonLanguageIdentifier = "typescript";
					highlightLanguageIdentifier = "ts";
					break;
				case "/vb":
					pistonLanguageIdentifier = "basic.net";
					highlightLanguageIdentifier = "vbnet";
					break;
				default:
					throw new ArgumentException("Command must be /c, /clojure, /crystal, /dart, /elixir, /go, /java, /kotlin, /lua, /pascal, /php, /python, /ruby, /rust, /scala, /swift, /julia, /sqlite3, /commonlisp, /cpp, /cs, /fs, /js, /ts, or /vb.", nameof(slashCommand));
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
					message: $"Untuk mengeksekusi program, silakan ketik `{slashCommand.Command}` diikuti code\\.",
					parseMode: ParseMode.MarkdownV2,
					commandMessageId: codeMessageId
				);
			}

			return new(
				pistonLanguageIdentifier: pistonLanguageIdentifier,
				highlightLanguageIdentifier: highlightLanguageIdentifier,
				code: code,
				isMentioned: slashCommand.IsMentioned,
				codeMessageId: new(codeMessageId),
				chat: slashCommand.Chat
			);
		}
	}
}
