using System.Net;
using System.Text.Json;
using BotNet.Commands.Eval;
using BotNet.Services.ClearScript;
using BotNet.Services.DynamicExpresso;
using Microsoft.ClearScript;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Eval {
	public sealed class EvalCommandHandler(
		ITelegramBotClient telegramBotClient,
		V8Evaluator v8Evaluator,
		CSharpEvaluator cSharpEvaluator
	) : ICommandHandler<EvalCommand> {
		private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new() { WriteIndented = true };

		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly V8Evaluator _v8Evaluator = v8Evaluator;
		private readonly CSharpEvaluator _cSharpEvaluator = cSharpEvaluator;

		public async Task Handle(EvalCommand command, CancellationToken cancellationToken) {
			string result;
			switch (command.Command) {
				case "/evaljs":
					try {
						result = await _v8Evaluator.EvaluateAsync(
							script: command.Code,
							cancellationToken: cancellationToken
						);
					} catch (ScriptEngineException exc) {
						await _telegramBotClient.SendTextMessageAsync(
							chatId: command.Chat.Id,
							text: "<code>" + WebUtility.HtmlEncode(exc.Message) + "</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: command.CodeMessageId,
							cancellationToken: cancellationToken
						);
						return;
					} catch (OperationCanceledException) {
						await _telegramBotClient.SendTextMessageAsync(
							chatId: command.Chat.Id,
							text: "<code>Timeout exceeded.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: command.CodeMessageId,
							cancellationToken: cancellationToken
						);
						return;
					} catch (JsonException exc) when (exc.Message.Contains("A possible object cycle was detected.")) {
						await _telegramBotClient.SendTextMessageAsync(
							chatId: command.Chat.Id,
							text: "<code>A possible object cycle was detected.</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: command.CodeMessageId,
							cancellationToken: cancellationToken
						);
						return;
					}
					break;
				case "/evalcs":
					try {
						object resultObject = _cSharpEvaluator.Evaluate(
							expression: command.Code
						);

						// Prettify result
						result = JsonSerializer.Serialize(resultObject, JSON_SERIALIZER_OPTIONS);
					} catch (Exception exc) {
						await _telegramBotClient.SendTextMessageAsync(
							chatId: command.Chat.Id,
							text: "<code>" + WebUtility.HtmlEncode(exc.Message) + "</code>",
							parseMode: ParseMode.Html,
							replyToMessageId: command.CodeMessageId,
							cancellationToken: cancellationToken
						);
						return;
					}
					break;
				default:
					throw new InvalidOperationException($"Unknown command: {command.Command}");
			}

			if (result.Length > 1000) {
				await _telegramBotClient.SendTextMessageAsync(
					chatId: command.Chat.Id,
					text: "<code>Result is too long.</code>",
					parseMode: ParseMode.Html,
					replyToMessageId: command.CodeMessageId,
					cancellationToken: cancellationToken
				);
			} else {
				await _telegramBotClient.SendTextMessageAsync(
					chatId: command.Chat.Id,
					text: result.Length >= 2 && result[0] == '"' && result[^1] == '"'
						? $"Expression:\n<code>{WebUtility.HtmlEncode(command.Code)}</code>\n\nString Result:\n<code>{WebUtility.HtmlEncode(result[1..^1].Replace("\\n", "\n"))}</code>"
						: $"Expression:\n<code>{WebUtility.HtmlEncode(command.Code)}</code>\n\nResult:\n<code>{WebUtility.HtmlEncode(result)}</code>",
					parseMode: ParseMode.Html,
					replyToMessageId: command.CodeMessageId,
					cancellationToken: cancellationToken
				);
			}
		}
	}
}
