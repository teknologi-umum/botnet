using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.SenderAggregate;

namespace BotNet.Commands.BotUpdate.Message {
	public sealed record AICallCommand : HumanMessageBase, ICommand {
		public static readonly ImmutableHashSet<string> CALL_SIGNS = [
			"AI",
			"Bot",
			"GPT",
			"Gemini",
			"Pakde"
		];

		public string CallSign { get; }

		private AICallCommand(
			MessageId messageId,
			ChatBase chat,
			HumanSender sender,
			string text,
			string? imageFileId,
			MessageBase? replyToMessage,
			string callSign
		) : base(
			messageId: messageId,
			chat: chat,
			sender: sender,
			text: text,
			imageFileId: imageFileId,
			replyToMessage: replyToMessage
		) {
			CallSign = callSign;
		}

		public static bool TryCreate(
			Telegram.Bot.Types.Message message,
			[NotNullWhen(true)] out AICallCommand? aiCallCommand
		) {
			// Chat must be private or group
			if (!ChatBase.TryCreate(message.Chat, out ChatBase? chat)) {
				aiCallCommand = null;
				return false;
			}

			// Sender must be a user
			if (message.From is not { } from
				|| !HumanSender.TryCreate(from, out HumanSender? sender)) {
				aiCallCommand = null;
				return false;
			}

			// Message must contain text or caption
			if ((message.Text ?? message.Caption) is not { } text) {
				aiCallCommand = null;
				return false;
			}

			// Message must start with call sign
			if (CALL_SIGNS.FirstOrDefault(callSign => text.StartsWith($"{callSign},", StringComparison.OrdinalIgnoreCase)) is not { } callSign) {
				aiCallCommand = null;
				return false;
			}

			aiCallCommand = new(
				messageId: new(message.MessageId),
				chat: chat,
				sender: sender,
				text: text[(callSign.Length + 1)..].Trim(),
				imageFileId: message.Photo?.LastOrDefault()?.FileId
					?? message.ReplyToMessage?.Sticker?.FileId,
				replyToMessage: message.ReplyToMessage is null
					? null
					: NormalMessage.FromMessage(message.ReplyToMessage),
				callSign: callSign
			);
			return true;
		}
	}
}
