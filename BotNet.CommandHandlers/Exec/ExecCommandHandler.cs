using System.Net;
using Mediator;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Exec;
using BotNet.Services.MarkdownV2;
using BotNet.Services.Piston;
using BotNet.Services.Piston.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Exec {
	public sealed class ExecCommandHandler(
		ITelegramBotClient telegramBotClient,
		PistonClient pistonClient,
		ILogger<ExecCommandHandler> logger
	) : ICommandHandler<ExecCommand> {
		public async ValueTask<Unit> Handle(ExecCommand command, CancellationToken cancellationToken) {
			// Ignore non-mentioned commands in home group
			if (command.Chat is HomeGroupChat
				&& !command.IsMentioned) {
				return default;
			}

			// Fire and forget
			BackgroundTask.Run(async () => {
				try {
					ExecuteResult result = await pistonClient.ExecuteAsync(
						language: command.PistonLanguageIdentifier,
						code: command.Code,
						cancellationToken: cancellationToken
					);

					if (result.Compile is { Code: not 0 }) {
						await telegramBotClient.SendMessage(
							chatId: command.Chat.Id,
							text: $"<code>{WebUtility.HtmlEncode(result.Compile.Stderr)}</code>",
							parseMode: ParseMode.Html,
							replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
							cancellationToken: cancellationToken
						);
					} else if (result.Run.Code != 0) {
						await telegramBotClient.SendMessage(
							chatId: command.Chat.Id,
							text: $"<code>{WebUtility.HtmlEncode(result.Run.Stderr)}</code>",
							parseMode: ParseMode.Html,
							replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
							cancellationToken: cancellationToken
						);
					} else if (result.Run.Output.Length > 1000 || result.Run.Output.Count(c => c == '\n') > 20) {
						await telegramBotClient.SendMessage(
						chatId: command.Chat.Id,
							text: "<code>Output is too long.</code>",
							parseMode: ParseMode.Html,
							replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
							cancellationToken: cancellationToken
						);
					} else {
						await telegramBotClient.SendMessage(
							chatId: command.Chat.Id,
							text: $"Code:\n```{command.HighlightLanguageIdentifier}\n{MarkdownV2Sanitizer.Sanitize(command.Code)}\n```\nOutput:\n```\n{MarkdownV2Sanitizer.Sanitize(result.Run.Output)}\n```",
							parseMode: ParseMode.MarkdownV2,
							replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
							cancellationToken: cancellationToken
						);
					}
#pragma warning disable CS0618 // Type or member is obsolete
				} catch (ExecutionEngineException exc) {
#pragma warning restore CS0618 // Type or member is obsolete
					await telegramBotClient.SendMessage(
						chatId: command.Chat.Id,
						text: "<code>" + WebUtility.HtmlEncode(exc.Message) + "</code>",
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
						cancellationToken: cancellationToken
					);
				} catch (OperationCanceledException) {
					await telegramBotClient.SendMessage(
						chatId: command.Chat.Id,
						text: "<code>Timeout exceeded.</code>",
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
						cancellationToken: cancellationToken
					);
				} catch (Exception exc) {
					logger.LogError(exc, "Unhandled exception while executing code.");
				}
			}, logger);

			return default;
		}
	}
}
