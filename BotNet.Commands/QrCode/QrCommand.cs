using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Common;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.QrCode {
	public sealed record QrCommand : ICommand {
		public string Url { get; }
		public ChatBase Chat { get; }
		public MessageId MessageId { get; }
		public SenderBase Sender { get; }

		private QrCommand(
			string url,
			ChatBase chat,
			MessageId messageId,
			SenderBase sender
		) {
			Url = url;
			Chat = chat;
			MessageId = messageId;
			Sender = sender;
		}

		public static QrCommand FromSlashCommand(SlashCommand slashCommand) {
			string url = slashCommand.Text.Trim();

			// Validate URL
			if (string.IsNullOrWhiteSpace(url)) {
				throw new UsageException(
					message: "Please provide a URL. Usage: /qr <url>",
					parseMode: ParseMode.Html,
					commandMessageId: slashCommand.MessageId
				);
			}

			if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) || 
			    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)) {
				throw new UsageException(
					message: "Please provide a valid HTTP or HTTPS URL.",
					parseMode: ParseMode.Html,
					commandMessageId: slashCommand.MessageId
				);
			}

			return new QrCommand(
				url: url,
				chat: slashCommand.Chat,
				messageId: slashCommand.MessageId,
				sender: slashCommand.Sender
			);
		}
	}
}
