using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotNet.Services.BubbleWrap {
	public sealed record BubbleWrapSheet(
		bool[,] Data
	) {
		public static BubbleWrapSheet EmptySheet { get; } = new(
			Data: new bool[4, 4].Setup(data => {
				for (int row = 0; row < 4; row++) {
					for (int col = 0; col < 4; col++) {
						data[row, col] = true;
					}
				}
			})
		);

		public static BubbleWrapSheet ParseSheetData(string sheetData = "FFFF") {
			bool[,] data = new bool[4, 4];
			for (int row = 0; row < 4; row++) {
				byte bitmap = byte.Parse(sheetData.Substring(row, 1), NumberStyles.HexNumber);
				for (int col = 0; col < 4; col++) {
					data[row, col] = (bitmap & (1 << (3 - col))) > 0;
				}
			}
			return new(data);
		}

		public BubbleWrapSheet Pop(int row, int col) {
			if (!Data[row, col]) return this;

			bool[,] data = (bool[,])Data.Clone();
			data[row, col] = false;
			return new(data);
		}

		public BubbleWrapSheet CombineWith(BubbleWrapSheet expectedSheet) {
			bool[,] data = new bool[4, 4];
			for (int row = 0; row < 4; row++) {
				for (int col = 0; col < 4; col++) {
					data[row, col] = Data[row, col] && expectedSheet.Data[row, col];
				}
			}
			return new(data);
		}

		public string ToSheetData() {
			StringBuilder callbackData = new();
			for (int row = 0; row < 4; row++) {
				int bitmap = 0;
				for (int col = 0; col < 4; col++) {
					if (Data[row, col]) {
						bitmap |= (byte)(1 << (3 - col));
					}
				}
				callbackData.Append(bitmap.ToString("X"));
			}
			return callbackData.ToString();
		}

		public InlineKeyboardMarkup ToKeyboardMarkup() {
			return new InlineKeyboardMarkup(
				Enumerable.Range(0, 4).Select(row => {
					return Enumerable.Range(0, 4).Select(col => {
						BubbleWrapSheet popped = Pop(row, col);
						string poppedCallbackData = "pop:" + popped.ToSheetData();
						return InlineKeyboardButton.WithCallbackData(
							text: Data[row, col] ? "⚪" : "💥",
							callbackData: poppedCallbackData
						);
					});
				})
			);
		}
	}
}
