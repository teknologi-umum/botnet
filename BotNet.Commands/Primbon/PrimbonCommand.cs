using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.Common;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Primbon {
	public sealed record PrimbonCommand : ICommand {
		public long ChatId { get; }
		public int CommandMessageId { get; }
		public long SenderId { get; }
		public DateOnly Date { get; }

		private PrimbonCommand(
			long chatId,
			int commandMessageId,
			long senderId,
			DateOnly date
		) {
			ChatId = chatId;
			CommandMessageId = commandMessageId;
			SenderId = senderId;
			Date = date;
		}

		public static PrimbonCommand FromSlashCommand(SlashCommand slashCommand) {
			if (slashCommand.Command != "/primbon") {
				throw new ArgumentException("Command must be /primbon.", nameof(slashCommand));
			}

			string arg = slashCommand.Text.ToLowerInvariant();
			DateOnly date;
			if (arg is "today" or "hari ini") {
				DateTime datetime = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7)).Date;
				date = new(datetime.Year, datetime.Month, datetime.Day);
			} else if (arg is "besok" or "tomorrow") {
				DateTime datetime = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7)).Date.AddDays(1);
				date = new(datetime.Year, datetime.Month, datetime.Day);
			} else if (arg is "kemarin" or "yesterday") {
				DateTime datetime = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7)).Date.AddDays(-1);
				date = new(datetime.Year, datetime.Month, datetime.Day);
			} else if (arg is "lusa") {
				DateTime datetime = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7)).Date.AddDays(2);
				date = new(datetime.Year, datetime.Month, datetime.Day);
			} else if (arg is not null) {
				if (!DateOnly.TryParseExact(arg, "d-M-yyyy", out date)
					&& !DateOnly.TryParseExact(arg, "yyyy-M-d", out date)
					&& !DateOnly.TryParseExact(arg, "d/M/yyyy", out date)
					&& !DateOnly.TryParseExact(arg, "yyyy/M/d", out date)
					&& !DateOnly.TryParseExact(arg, "d MMM yyyy", out date)
					&& !DateOnly.TryParseExact(arg, "d MMMM yyyy", out date)) {
					throw new UsageException(
						message: "<code>Format tanggal salah.</code>",
						parseMode: ParseMode.Html,
						commandMessageId: slashCommand.MessageId
					);
				}
			} else {
				DateTime datetime = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7)).Date;
				date = new(datetime.Year, datetime.Month, datetime.Day);
			}

			return new(
				chatId: slashCommand.ChatId,
				commandMessageId: slashCommand.MessageId,
				senderId: slashCommand.SenderId,
				date: date
			);
		}
	}
}
