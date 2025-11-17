using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Common;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Benchmark {
	public sealed record BenchmarkCommand : ICommand {
		public string[] Languages { get; }
		public ChatBase Chat { get; }
		public MessageId MessageId { get; }
		public SenderBase Sender { get; }

		private BenchmarkCommand(
			string[] languages,
			ChatBase chat,
			MessageId messageId,
			SenderBase sender
		) {
			Languages = languages;
			Chat = chat;
			MessageId = messageId;
			Sender = sender;
		}

		public static BenchmarkCommand FromSlashCommand(SlashCommand slashCommand) {
			string text = slashCommand.Text.Trim();

			// Validate that languages are provided
			if (string.IsNullOrWhiteSpace(text)) {
				throw new UsageException(
					message: "Please provide languages to compare. Usage: /benchmark C# C++",
					parseMode: ParseMode.Html,
					commandMessageId: slashCommand.MessageId
				);
			}

			// Split by whitespace to get language names
			string[] languages = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			if (languages.Length < 2) {
				throw new UsageException(
					message: "Please provide at least 2 languages to compare.",
					parseMode: ParseMode.Html,
					commandMessageId: slashCommand.MessageId
				);
			}

			return new BenchmarkCommand(
				languages: languages,
				chat: slashCommand.Chat,
				messageId: slashCommand.MessageId,
				sender: slashCommand.Sender
			);
		}
	}
}
