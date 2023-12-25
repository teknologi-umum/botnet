using System.Diagnostics.CodeAnalysis;
using BotNet.Commands.BotUpdate.CallbackQuery;

namespace BotNet.Commands.Pop {
	public sealed record BubbleWrapCallback : CallbackQueryBase, ICommand {
		public string SheetData => CallbackData![4..];

		private BubbleWrapCallback(
			int messageId,
			long chatId,
			string callbackData
		) : base(
			messageId: messageId,
			chatId: chatId,
			callbackData: callbackData
		) { }

		public static bool TryCreate(
			Telegram.Bot.Types.CallbackQuery callbackQuery,
			[NotNullWhen(true)] out BubbleWrapCallback? bubbleWrapCallback
		) {
			// Must contain callback data and reference to message
			if (callbackQuery is not {
				Data: string callbackData,
				Message.MessageId: int messageId,
				Message.Chat.Id: long chatId
			}) {
				bubbleWrapCallback = null;
				return false;
			}

			// Callback data must start with "pop:"
			if (!callbackData.StartsWith("pop:")) {
				bubbleWrapCallback = null;
				return false;
			}

			// Callback data must contain sheet data
			if (callbackData[4..] is not { Length: 4 } sheetData
				|| !sheetData.All(c => char.IsAsciiHexDigitUpper(c))) {
				bubbleWrapCallback = null;
				return false;
			}

			bubbleWrapCallback = new(
				messageId: messageId,
				chatId: chatId,
				callbackData: callbackData
			);
			return true;
		}
	}
}
