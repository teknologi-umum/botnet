using System.Net;
using System.Text.Json;
using Mediator;
using BotNet.Commands.Eval;
using BotNet.Services.ClearScript;
using BotNet.Services.DynamicExpresso;
using Microsoft.ClearScript;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Eval {
	public sealed class EvalCommandHandler(
		ITelegramBotClient telegramBotClient,
		V8Evaluator v8Evaluator,
		CSharpEvaluator cSharpEvaluator
	) : ICommandHandler<EvalCommand> {
		private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

		public async ValueTask<Unit> Handle(EvalCommand command, CancellationToken cancellationToken) {
			string result;
			switch (command.Command) {
				case "/evaljs":
					try {
						result = await v8Evaluator.EvaluateAsync(
							script: command.Code,
							cancellationToken: cancellationToken
						);
					} catch (ScriptEngineException exc) {
						await telegramBotClient.SendMessage(
							chatId: command.Chat.Id,
							text: "<code>" + WebUtility.HtmlEncode(exc.Message) + "</code>",
							parseMode: ParseMode.Html,
							replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
							cancellationToken: cancellationToken
						);
						return default;
					} catch (OperationCanceledException) {
						await telegramBotClient.SendMessage(
							chatId: command.Chat.Id,
							text: "<code>Timeout exceeded.</code>",
							parseMode: ParseMode.Html,
							replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
							cancellationToken: cancellationToken
						);
						return default;
					} catch (JsonException exc) when (exc.Message.Contains("A possible object cycle was detected.")) {
						await telegramBotClient.SendMessage(
							chatId: command.Chat.Id,
							text: "<code>A possible object cycle was detected.</code>",
							parseMode: ParseMode.Html,
							replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
							cancellationToken: cancellationToken
						);
						return default;
		return default;
					}
					break;
				case "/evalcs":
					try {
						object resultObject = cSharpEvaluator.Evaluate(
							expression: command.Code
						);

						// Prettify result
						result = JsonSerializer.Serialize(resultObject, JsonSerializerOptions);
					} catch (Exception exc) {
						await telegramBotClient.SendMessage(
							chatId: command.Chat.Id,
							text: "<code>" + WebUtility.HtmlEncode(exc.Message) + "</code>",
							parseMode: ParseMode.Html,
							replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
							cancellationToken: cancellationToken
						);
						return default;
					}
					break;
				default:
					throw new InvalidOperationException($"Unknown command: {command.Command}");
			}

			if (result.Length > 1000) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: "<code>Result is too long.</code>",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
					cancellationToken: cancellationToken
				);
			} else {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: result.Length >= 2 && result[0] == '"' && result[^1] == '"'
						? $"Expression:\n<code>{WebUtility.HtmlEncode(command.Code)}</code>\n\nString Result:\n<code>{WebUtility.HtmlEncode(result[1..^1].Replace("\\n", "\n"))}</code>"
						: $"Expression:\n<code>{WebUtility.HtmlEncode(command.Code)}</code>\n\nResult:\n<code>{WebUtility.HtmlEncode(result)}</code>",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters { MessageId = command.CodeMessageId },
					cancellationToken: cancellationToken
				);
			}
	return default;
		}
	}
}
