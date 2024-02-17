using System.Text;
using BotNet.Commands.SQL;
using BotNet.Services.SQL;
using BotNet.Services.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using SqlParser.Ast;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.SQL {
	public sealed class SQLCommandHandler(
		ITelegramBotClient telegramBotClient,
		IServiceProvider serviceProvider
	) : ICommandHandler<SQLCommand> {
		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly IServiceProvider _serviceProvider = serviceProvider;

		public async Task Handle(SQLCommand command, CancellationToken cancellationToken) {
			if (command.SelectStatement.Query.Body.AsSelectExpression().Select.From is not { } froms
				|| froms.Count == 0) {
				await _telegramBotClient.SendTextMessageAsync(
					chatId: command.Chat.Id,
					text: "<code>No FROM clause found.</code>",
					parseMode: ParseMode.Html,
					replyToMessageId: command.SQLMessageId,
					cancellationToken: cancellationToken
				);
				return;
			}

			// Collect table names from query
			HashSet<string> tables = new();
			foreach (TableWithJoins from in froms) {
				if (from.Relation != null) {
					CollectTableNames(ref tables, from.Relation);
				}

				if (from.Joins != null) {
					foreach (Join join in from.Joins) {
						if (join.Relation != null) {
							CollectTableNames(ref tables, join.Relation);
						}
					}
				}
			}

			// Create scoped for scoped database
			using IServiceScope serviceScope = _serviceProvider.CreateScope();

			// Load tables into memory
			foreach (string table in tables) {
				IScopedDataSource? dataSource = serviceScope.ServiceProvider.GetKeyedService<IScopedDataSource>(table);
				if (dataSource == null) {
					await _telegramBotClient.SendTextMessageAsync(
						chatId: command.Chat.Id,
						text: $$"""
						<code>Table '{{table}}' not found. Available tables are:
						- pileg_dpr_dapil
						- pileg_dpr_&lt;kodedapil&gt;
						- pileg_dpr_provinsi
						- pilpres
						- vps
						</code>
						""",
						parseMode: ParseMode.Html,
						replyToMessageId: command.SQLMessageId,
						cancellationToken: cancellationToken
					);
					return;
				}

				await dataSource.LoadTableAsync(cancellationToken);
			}

			// Execute query
			using ScopedDatabase scopedDatabase = serviceScope.ServiceProvider.GetRequiredService<ScopedDatabase>();
			StringBuilder resultBuilder = new();
			int rows = 0;

			try {
				scopedDatabase.ExecuteReader(
					commandText: command.RawStatement,
					readAction: (reader) => {
						string[] values = new string[reader.FieldCount];

						// Get column names
						for (int i = 0; i < reader.FieldCount; i++) {
							values[i] = '"' + reader.GetName(i).Replace("\"", "\"\"") + '"';
						}
						resultBuilder.AppendLine(string.Join(',', values));

						// Get rows
						while (reader.Read()) {
							for (int i = 0; i < reader.FieldCount; i++) {
								if (reader.IsDBNull(i)) {
									values[i] = "";
									continue;
								}

								Type fieldType = reader.GetFieldType(i);
								if (fieldType == typeof(string)) {
									values[i] = '"' + reader.GetString(i).Replace("\"", "\"\"") + '"';
								} else if (fieldType == typeof(int)) {
									values[i] = reader.GetInt32(i).ToString();
								} else if (fieldType == typeof(long)) {
									values[i] = reader.GetInt64(i).ToString();
								} else if (fieldType == typeof(float)) {
									values[i] = reader.GetFloat(i).ToString();
								} else if (fieldType == typeof(double)) {
									values[i] = reader.GetDouble(i).ToString();
								} else if (fieldType == typeof(decimal)) {
									values[i] = reader.GetDecimal(i).ToString();
								} else if (fieldType == typeof(bool)) {
									values[i] = reader.GetBoolean(i).ToString();
								} else if (fieldType == typeof(DateTime)) {
									values[i] = reader.GetDateTime(i).ToString();
								} else if (fieldType == typeof(byte[])) {
									values[i] = BitConverter.ToString(reader.GetFieldValue<byte[]>(i)).Replace("-", "");
								} else {
									values[i] = reader[i].ToString();
								}
							}
							resultBuilder.AppendLine(string.Join(',', values));
							rows++;
						}
					}
				);
			} catch (SqliteException exc) {
				await _telegramBotClient.SendTextMessageAsync(
					chatId: command.Chat.Id,
					text: "<code>" + exc.Message.Replace("SQLite Error", "Error") + "</code>",
					parseMode: ParseMode.Html,
					replyToMessageId: command.SQLMessageId,
					cancellationToken: cancellationToken
				);
				return;
			}

			// Send result
			string csvResult = resultBuilder.ToString();
			if (csvResult.Length > 4000) {
				await _telegramBotClient.SendDocumentAsync(
					chatId: command.Chat.Id,
					caption: $"{rows} rows affected",
					document: new InputFileStream(new MemoryStream(Encoding.UTF8.GetBytes(csvResult)), "result.csv"),
					replyToMessageId: command.SQLMessageId,
					cancellationToken: cancellationToken
				);
			} else {
				await _telegramBotClient.SendTextMessageAsync(
					chatId: command.Chat.Id,
					text: "```csv\n" + resultBuilder.ToString() + $"```\n{rows} rows affected",
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: command.SQLMessageId,
					cancellationToken: cancellationToken
				);
			}
		}

		private static void CollectTableNames(ref HashSet<string> tables, TableFactor tableFactor) {
			switch (tableFactor) {
				case TableFactor.Derived derived:
					if (derived.SubQuery.Body.AsSelectExpression().Select.From is { } derivedFroms) {
						foreach (TableWithJoins derivedFrom in derivedFroms) {
							if (derivedFrom.Relation != null) {
								CollectTableNames(ref tables, derivedFrom.Relation);
							}

							if (derivedFrom.Joins != null) {
								foreach (Join join in derivedFrom.Joins) {
									if (join.Relation != null) {
										CollectTableNames(ref tables, join.Relation);
									}
								}
							}
						}
					}
					break;
				case TableFactor.Function function:
					break;
				case TableFactor.JsonTable jsonTable:
					break;
				case TableFactor.NestedJoin nestedJoin:
					if (nestedJoin.TableWithJoins != null) {
						if (nestedJoin.TableWithJoins.Relation != null) {
							CollectTableNames(ref tables, nestedJoin.TableWithJoins.Relation);
						}

						if (nestedJoin.TableWithJoins.Joins != null) {
							foreach (Join join in nestedJoin.TableWithJoins.Joins) {
								if (join.Relation != null) {
									CollectTableNames(ref tables, join.Relation);
								}
							}
						}
					}
					break;
				case TableFactor.Pivot pivot:
					CollectTableNames(ref tables, pivot.TableFactor);
					break;
				case TableFactor.Table table:
					tables.Add(table.Name.ToString());
					break;
				case TableFactor.TableFunction tableFunction:
					break;
				case TableFactor.UnNest unNest:
					break;
				case TableFactor.Unpivot unpivot:
					tables.Add(unpivot.Name.ToString());
					break;
			}
		}
	}
}
