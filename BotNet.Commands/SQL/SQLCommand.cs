using System.Diagnostics.CodeAnalysis;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.CommandPrioritization;
using SqlParser;
using SqlParser.Ast;

namespace BotNet.Commands.SQL {
	public sealed record SQLCommand : ICommand {
		public string RawStatement { get; }
		public Statement.Select SelectStatement { get; }
		public MessageId SQLMessageId { get; }
		public ChatBase Chat { get; }

		private SQLCommand(
			string rawStatement,
			Statement.Select selectStatement,
			MessageId sqlMessageId,
			ChatBase chat
		) {
			RawStatement = rawStatement;
			SelectStatement = selectStatement;
			SQLMessageId = sqlMessageId;
			Chat = chat;
		}

		public static bool TryCreate(
			Telegram.Bot.Types.Message message,
			CommandPriorityCategorizer commandPriorityCategorizer,
			[NotNullWhen(true)] out SQLCommand? sqlCommand
		) {
			// Must start with select
			if (message.Text is not { } text || !text.StartsWith("select", StringComparison.OrdinalIgnoreCase)) {
				sqlCommand = null;
				return false;
			}

			// Chat must be private or group
			if (!ChatBase.TryCreate(message.Chat, commandPriorityCategorizer, out ChatBase? chat)) {
				sqlCommand = null;
				return false;
			}

			// Must be a valid SQL statement
			Sequence<Statement> ast;
			try {
				ast = new SqlParser.Parser().ParseSql(text);
			} catch {
				sqlCommand = null;
				return false;
			}

			// Can only contain one statement
			if (ast.Count != 1) {
				sqlCommand = null;
				return false;
			}

			// Must be a SELECT statement
			if (ast[0] is not Statement.Select selectStatement) {
				sqlCommand = null;
				return false;
			}

			sqlCommand = new(
				rawStatement: text,
				selectStatement: selectStatement,
				sqlMessageId: new(message.MessageId),
				chat: chat
			);
			return true;
		}
	}
}
