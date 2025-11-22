using System.Collections.Immutable;
using BotNet.Commands.BotUpdate.InlineQuery;
using Mediator;
using BotNet.Services.Brainfuck;
using BotNet.Services.CopyPasta;
using BotNet.Services.FancyText;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.InlineQueryResults;

namespace BotNet.CommandHandlers.BotUpdate.InlineQuery {
	public sealed class InlineQueryUpdateHandler(
		ITelegramBotClient telegramBotClient,
		BrainfuckTranspiler brainfuckTranspiler,
		ILogger<InlineQueryUpdateHandler> logger
	) : ICommandHandler<InlineQueryUpdate> {
		public ValueTask<Unit> Handle(InlineQueryUpdate command, CancellationToken cancellationToken) {
			// Fire and forget
			BackgroundTask.Run(async () => {
				// Query must not be empty
				if (command.InlineQuery.Query.Trim() is not { Length: > 0 } query) {
					return;
				}

				List<InlineQueryResult> results = [];

				// Find copypasta
				if (CopyPastaLookup.TryGetAutoText(
					key: query.ToLowerInvariant(),
					values: out ImmutableList<string>? autoTexts
				)) {
					foreach (string pasta in autoTexts) {
						results.Add(new InlineQueryResultArticle(
							id: Guid.NewGuid().ToString("N"),
							title: pasta,
							inputMessageContent: new InputTextMessageContent(pasta)
						));
					}
				}

				// Generate fancy texts
				foreach (FancyTextStyle style in Enum.GetValues<FancyTextStyle>()) {
					string fancyText = await FancyTextGenerator.GenerateAsync(query, style, cancellationToken);
					results.Add(new InlineQueryResultArticle(
						id: Guid.NewGuid().ToString("N"),
						title: fancyText,
						inputMessageContent: new InputTextMessageContent(fancyText)
					));
				}

				// Generate brainfuck code
				string brainfuckCode = brainfuckTranspiler.TranspileBrainfuck(query);
				results.Add(new InlineQueryResultArticle(
					id: Guid.NewGuid().ToString("N"),
					title: brainfuckCode,
					inputMessageContent: new InputTextMessageContent(brainfuckCode)
				));

				// Send results
				await telegramBotClient.AnswerInlineQuery(
					inlineQueryId: command.InlineQuery.Id,
					results: results,
					cancellationToken: cancellationToken
				);
			}, logger);

			return default;
		}
	}
}
