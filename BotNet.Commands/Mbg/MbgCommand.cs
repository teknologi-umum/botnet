using System.Globalization;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Common;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Mbg {
	public sealed record MbgCommand : ICommand {
		public decimal RupiahAmount { get; }
		public ChatBase Chat { get; }
		public MessageId MessageId { get; }
		public SenderBase Sender { get; }

		private MbgCommand(
			decimal rupiahAmount,
			ChatBase chat,
			MessageId messageId,
			SenderBase sender
		) {
			RupiahAmount = rupiahAmount;
			Chat = chat;
			MessageId = messageId;
			Sender = sender;
		}

		public static MbgCommand FromSlashCommand(SlashCommand slashCommand) {
			string text = slashCommand.Text.Trim();

			if (string.IsNullOrWhiteSpace(text)) {
				throw new UsageException(
					message: "Masukkan jumlah rupiah. Contoh: <code>/mbg 1000000</code> atau <code>/mbg 1,000,000</code>",
					parseMode: ParseMode.Html,
					commandMessageId: slashCommand.MessageId
				);
			}

			decimal rupiahAmount;
			try {
				rupiahAmount = ParseRupiah(text);
			} catch (FormatException) {
				throw new UsageException(
					message: "Format tidak valid. Contoh: <code>/mbg 1000000</code> atau <code>/mbg 1,000,000</code>",
					parseMode: ParseMode.Html,
					commandMessageId: slashCommand.MessageId
				);
			} catch (OverflowException) {
				throw new UsageException(
					message: "Jumlah rupiah terlalu besar.",
					parseMode: ParseMode.Html,
					commandMessageId: slashCommand.MessageId
				);
			}

			if (rupiahAmount <= 0) {
				throw new UsageException(
					message: "Jumlah rupiah harus lebih dari 0.",
					parseMode: ParseMode.Html,
					commandMessageId: slashCommand.MessageId
				);
			}

			return new MbgCommand(
				rupiahAmount: rupiahAmount,
				chat: slashCommand.Chat,
				messageId: slashCommand.MessageId,
				sender: slashCommand.Sender
			);
		}

		internal static decimal ParseRupiah(string text) {
			string cleaned = text.Replace(",", "");
			return decimal.Parse(cleaned, CultureInfo.InvariantCulture);
		}

		public static string FormatMbgTime(decimal rupiahAmount) {
			const decimal rupiahPerMbgDay = 1_200_000_000_000M;
			const decimal secondsPerMinute = 60M;
			const decimal secondsPerHour = 3_600M;
			const decimal secondsPerDay = 86_400M;
			const decimal secondsPerYear = secondsPerDay * 365M;

			decimal totalSeconds = rupiahAmount * secondsPerDay / rupiahPerMbgDay;

			if (totalSeconds < secondsPerMinute) {
				string detik = totalSeconds.ToString("0.##", CultureInfo.InvariantCulture);
				return $"{detik} detik MBG";
			}

			if (totalSeconds < secondsPerHour) {
				long menit = (long)Math.Floor(totalSeconds / secondsPerMinute);
				return $"{menit} menit MBG";
			}

			if (totalSeconds < secondsPerDay) {
				long jam = (long)Math.Floor(totalSeconds / secondsPerHour);
				return $"{jam} jam MBG";
			}

			if (totalSeconds < secondsPerYear) {
				long hari = (long)Math.Floor(totalSeconds / secondsPerDay);
				long sisaJam = (long)Math.Floor(totalSeconds % secondsPerDay / secondsPerHour);
				if (sisaJam > 0) {
					return $"{hari} hari {sisaJam} jam MBG";
				}
				return $"{hari} hari MBG";
			}

			{
				long tahun = (long)Math.Floor(totalSeconds / secondsPerYear);
				long sisaHari = (long)Math.Floor(totalSeconds % secondsPerYear / secondsPerDay);
				if (sisaHari > 0) {
					return $"{tahun} tahun {sisaHari} hari MBG";
				}
				return $"{tahun} tahun MBG";
			}
		}
	}
}
