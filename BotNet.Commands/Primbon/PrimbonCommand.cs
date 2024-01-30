using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Common;
using BotNet.Commands.SenderAggregate;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.Primbon {
	public sealed record PrimbonCommand : ICommand {
		public DateOnly Date { get; }
		public MessageId CommandMessageId { get; }
		public ChatBase Chat { get; }
		public HumanSender Sender { get; }

		private PrimbonCommand(
			DateOnly date,
			MessageId commandMessageId,
			ChatBase chat,
			HumanSender sender
		) {
			Date = date;
			CommandMessageId = commandMessageId;
			Chat = chat;
			Sender = sender;
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
			} else if (!string.IsNullOrWhiteSpace(arg)) {
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
				date: date,
				commandMessageId: slashCommand.MessageId,
				chat: slashCommand.Chat,
				sender: slashCommand.Sender
			);
		}
	}
}
