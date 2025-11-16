using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Common;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Pick {
	public sealed record PickCommand : ICommand {
		public string[] Options { get; }
		public ChatBase Chat { get; }
		public MessageId MessageId { get; }
		public SenderBase Sender { get; }

		private PickCommand(
			string[] options,
			ChatBase chat,
			MessageId messageId,
			SenderBase sender
		) {
			Options = options;
			Chat = chat;
			MessageId = messageId;
			Sender = sender;
		}

		public static PickCommand FromSlashCommand(SlashCommand slashCommand) {
			string text = slashCommand.Text.Trim();

			// Validate that options are provided
			if (string.IsNullOrWhiteSpace(text)) {
				throw new UsageException(
					message: "Please provide options to pick from. Usage: /pick pizza sushi burger",
					parseMode: ParseMode.Html,
					commandMessageId: slashCommand.MessageId
				);
			}

			string[] options;

			// Check if comma-separated format is used (commas outside quotes)
			if (ContainsCommaOutsideQuotes(text)) {
				// Split by comma and trim each option
				options = SplitByCommaOutsideQuotes(text)
					.Select(opt => opt.Trim().Trim('"'))
					.Where(opt => !string.IsNullOrWhiteSpace(opt))
					.ToArray();
			} else {
				// Parse with support for quoted strings
				options = ParseOptions(text);
			}

			if (options.Length < 2) {
				throw new UsageException(
					message: "Please provide at least 2 options to pick from.",
					parseMode: ParseMode.Html,
					commandMessageId: slashCommand.MessageId
				);
			}

			return new PickCommand(
				options: options,
				chat: slashCommand.Chat,
				messageId: slashCommand.MessageId,
				sender: slashCommand.Sender
			);
		}

		internal static bool ContainsCommaOutsideQuotes(string text) {
			bool inQuotes = false;
			for (int i = 0; i < text.Length; i++) {
				if (text[i] == '"') {
					inQuotes = !inQuotes;
				} else if (text[i] == ',' && !inQuotes) {
					return true;
				}
			}
			return false;
		}

		internal static string[] SplitByCommaOutsideQuotes(string text) {
			List<string> parts = new();
			bool inQuotes = false;
			int startIndex = 0;

			for (int i = 0; i < text.Length; i++) {
				if (text[i] == '"') {
					inQuotes = !inQuotes;
				} else if (text[i] == ',' && !inQuotes) {
					parts.Add(text.Substring(startIndex, i - startIndex));
					startIndex = i + 1;
				}
			}

			// Add the last part
			if (startIndex < text.Length) {
				parts.Add(text.Substring(startIndex));
			}

			return parts.ToArray();
		}

		internal static string[] ParseOptions(string text) {
			List<string> options = new();
			bool inQuotes = false;
			int startIndex = 0;

			for (int i = 0; i < text.Length; i++) {
				char c = text[i];

				if (c == '"') {
					if (inQuotes) {
						// End of quoted string
						string option = text.Substring(startIndex + 1, i - startIndex - 1).Trim();
						if (!string.IsNullOrWhiteSpace(option)) {
							options.Add(option);
						}
						startIndex = i + 1;
						inQuotes = false;
					} else {
						// Start of quoted string
						inQuotes = true;
						startIndex = i;
					}
				} else if (char.IsWhiteSpace(c) && !inQuotes) {
					// Space outside quotes - end of option
					if (i > startIndex) {
						string option = text.Substring(startIndex, i - startIndex).Trim();
						if (!string.IsNullOrWhiteSpace(option)) {
							options.Add(option);
						}
					}
					startIndex = i + 1;
				}
			}

			// Add last option
			if (startIndex < text.Length) {
				string lastOption = text.Substring(startIndex).Trim().Trim('"');
				if (!string.IsNullOrWhiteSpace(lastOption)) {
					options.Add(lastOption);
				}
			}

			return options.ToArray();
		}
	}
}
