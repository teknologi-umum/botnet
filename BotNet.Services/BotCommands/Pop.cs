using System.Globalization;
using System.Linq;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotNet.Services.BotCommands {
	public static class Pop {
		public static InlineKeyboardMarkup GenerateBubbleWrap(bool[,] data) {
			return new InlineKeyboardMarkup(
				Enumerable.Range(0, 4).Select(row => {
					return Enumerable.Range(0, 4).Select(col => {
						bool[,] popped = (bool[,])data.Clone();
						popped[row, col] = false;
						string poppedCallbackData = ToCallbackData(popped);
						return InlineKeyboardButton.WithCallbackData(
							text: data[row, col] ? "⚪" : "💥",
							callbackData: poppedCallbackData
						);
					});
				})
			);
		}

		public static bool[,] NewSheet() {
			bool[,] data = new bool[4, 4];
			for (int row = 0; row < 4; row++) {
				for (int col = 0; col < 4; col++) {
					data[row, col] = true;
				}
			}
			return data;
		}

		public static bool[,] ParseCallbackData(string callbackData = "FFFF") {
			bool[,] data = new bool[4, 4];
			for (int row = 0; row < 4; row++) {
				byte bitmap = byte.Parse(callbackData.Substring(row, 1), NumberStyles.HexNumber);
				for (int col = 0; col < 4; col++) {
					data[row, col] = (bitmap & (1 << (3 - col))) > 0;
				}
			}
			return data;
		}

		public static string ToCallbackData(bool[,] data) {
			StringBuilder callbackData = new();
			for (int row = 0; row < 4; row++) {
				int bitmap = 0;
				for (int col = 0; col < 4; col++) {
					if (data[row, col]) {
						bitmap |= (byte)(1 << (3 - col));
					}
				}
				callbackData.Append(bitmap.ToString("X"));
			}
			return callbackData.ToString();
		}
	}
}
