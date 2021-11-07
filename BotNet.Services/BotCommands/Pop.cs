using System;
using System.Linq;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotNet.Services.BotCommands {
	public static class Pop {
		public static InlineKeyboardMarkup GenerateBubbleWrap(bool[,] data) {
			return new InlineKeyboardMarkup(
				Enumerable.Range(0, 8).Select(row => {
					byte bitmap = GetBitmap(data, row);
					return Enumerable.Range(0, 8).Select(col => {
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
			bool[,] data = new bool[8, 8];
			for (int row = 0; row < 8; row++) {
				for (int col = 0; col < 8; col++) {
					data[row, col] = true;
				}
			}
			return data;
		}

		public static bool[,] ParseCallbackData(string callbackData = "FFFFFFFFFFFFFFFF") {
			bool[,] data = new bool[8, 8];
			for (int row = 0; row < 8; row++) {
				byte bitmap = Convert.ToByte(callbackData.Substring(row * 2, 2), 16);
				for (int col = 0; col < 8; col++) {
					data[row, col] = (bitmap & (1 << (7 - col))) > 0;
				}
			}
			return data;
		}

		public static string ToCallbackData(bool[,] data) {
			StringBuilder callbackData = new();
			for (int row = 0; row < 8; row++) {
				callbackData.AppendFormat("{0:X2}", GetBitmap(data, row));
			}
			return callbackData.ToString();
		}

		private static byte GetBitmap(bool[,] data, int row) {
			byte bitmap = 0x00;
			for (int col = 0; col < 8; col++) {
				if (data[row, col]) {
					bitmap |= (byte)(1 << (7 - col));
				}
			}
			return bitmap;
		}
	}
}
