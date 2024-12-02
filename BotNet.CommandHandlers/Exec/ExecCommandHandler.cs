using System.Net;
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
		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly PistonClient _pistonClient = pistonClient;
		private readonly ILogger<ExecCommandHandler> _logger = logger;

		public Task Handle(ExecCommand command, CancellationToken cancellationToken) {
			// Ignore non-mentioned commands in home group
			if (command.Chat is HomeGroupChat
				&& !command.IsMentioned) {
				return Task.CompletedTask;
			}

			// Fire and forget
			Task.Run(async () => {
				try {
					ExecuteResult result = await _pistonClient.ExecuteAsync(
						language: command.PistonLanguageIdentifier,
						code: command.Code,
						cancellationToken: cancellationToken
					);

					if (result.Compile is { Code: not 0 }) {
						await _telegramBotClient.SendMessage(
							chatId: command.Chat.Id,
							text: $"<code>{WebUtility.HtmlEncode(result.Compile.Stderr)}</code>",
							parseMode: ParseMode.Html,
							replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
							cancellationToken: cancellationToken
						);
					} else if (result.Run.Code != 0) {
						await _telegramBotClient.SendMessage(
							chatId: command.Chat.Id,
							text: $"<code>{WebUtility.HtmlEncode(result.Run.Stderr)}</code>",
							parseMode: ParseMode.Html,
							replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
							cancellationToken: cancellationToken
						);
					} else if (result.Run.Output.Length > 1000 || result.Run.Output.Count(c => c == '\n') > 20) {
						await _telegramBotClient.SendMessage(
						chatId: command.Chat.Id,
							text: "<code>Output is too long.</code>",
							parseMode: ParseMode.Html,
							replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
							cancellationToken: cancellationToken
						);
					} else {
						await _telegramBotClient.SendMessage(
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
					await _telegramBotClient.SendMessage(
						chatId: command.Chat.Id,
						text: "<code>" + WebUtility.HtmlEncode(exc.Message ?? "Unknown error") + "</code>",
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
						cancellationToken: cancellationToken
					);
				} catch (OperationCanceledException) {
					await _telegramBotClient.SendMessage(
						chatId: command.Chat.Id,
						text: "<code>Timeout exceeded.</code>",
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
						cancellationToken: cancellationToken
					);
				} catch (Exception exc) {
					_logger.LogError(exc, "Unhandled exception while executing code.");
				}
			});

			return Task.CompletedTask;
		}
	}
}
