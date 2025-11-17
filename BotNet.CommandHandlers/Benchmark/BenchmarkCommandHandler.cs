using BotNet.Commands.Benchmark;
using BotNet.Services.MarkdownV2;
using BotNet.Services.RateLimit;
using BotNet.Services.TechEmpower;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Benchmark {
	public sealed class BenchmarkCommandHandler(
		ITelegramBotClient telegramBotClient,
		TechEmpowerScraper techEmpowerScraper,
		ILogger<BenchmarkCommandHandler> logger
	) : ICommandHandler<BenchmarkCommand> {
		private static readonly RateLimiter RateLimiter = RateLimiter.PerUserPerChat(3, TimeSpan.FromMinutes(5));

		public async Task Handle(BenchmarkCommand command, CancellationToken cancellationToken) {
			// Rate limiting
			try {
				RateLimiter.ValidateActionRate(command.Chat.Id, command.Sender.Id);
			} catch (RateLimitExceededException exc) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"Rate limit exceeded. Try again {exc.Cooldown}.",
					cancellationToken: cancellationToken
				);
				return;
			}

			try {
				// Fetch benchmark results
				BenchmarkResult[] allResults = await techEmpowerScraper.GetCompositeScoresAsync(cancellationToken);

				// Get best results for each requested language
				List<(string Language, BenchmarkResult? Result)> languageResults = new();
				foreach (string language in command.Languages) {
					BenchmarkResult? result = techEmpowerScraper.GetBestResultForLanguage(allResults, language);
					languageResults.Add((language, result));
				}

				// Check if any languages were not found
				List<string> notFound = languageResults
					.Where(lr => lr.Result is null)
					.Select(lr => lr.Language)
					.ToList();

				if (notFound.Count > 0) {
					string notFoundList = string.Join(", ", notFound.Select(MarkdownV2Sanitizer.Sanitize));
					await telegramBotClient.SendMessage(
						chatId: command.Chat.Id,
						text: $"Could not find benchmark results for: {notFoundList}",
						parseMode: ParseMode.MarkdownV2,
						replyParameters: new Telegram.Bot.Types.ReplyParameters {
							MessageId = command.MessageId
						},
						cancellationToken: cancellationToken
					);
					return;
				}

				// Compare the languages
				string response = FormatComparisonResponse(languageResults);

				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: response,
					parseMode: ParseMode.MarkdownV2,
					replyParameters: new Telegram.Bot.Types.ReplyParameters {
						MessageId = command.MessageId
					},
					cancellationToken: cancellationToken
				);
			} catch (Exception exc) {
				logger.LogError(exc, "Failed to fetch or process TechEmpower benchmark data");
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: "Failed to fetch benchmark data. Please try again later.",
					cancellationToken: cancellationToken
				);
			}
		}

		private static string FormatComparisonResponse(List<(string Language, BenchmarkResult? Result)> languageResults) {
			// Sort by score (descending)
			List<(string Language, BenchmarkResult Result)> validResults = languageResults
				.Where(lr => lr.Result is not null)
				.Select(lr => (Language: lr.Language, Result: lr.Result!))
				.OrderByDescending(lr => lr.Result.Score)
				.ToList();

			if (validResults.Count < 2) {
				return "Need at least 2 valid languages to compare.";
			}

			// Calculate comparison
			(string Language, BenchmarkResult Result) fastest = validResults[0];
			(string Language, BenchmarkResult Result) slowest = validResults[^1];

			double percentageFaster = ((fastest.Result.Score - slowest.Result.Score) / slowest.Result.Score) * 100;

			// Build response
			System.Text.StringBuilder sb = new();
			sb.AppendLine("*TechEmpower Benchmark Results:*");
			sb.AppendLine();

			foreach ((string Language, BenchmarkResult Result) item in validResults) {
				string languageName = MarkdownV2Sanitizer.Sanitize(item.Language);
				string framework = MarkdownV2Sanitizer.Sanitize(item.Result.Framework);
				string score = MarkdownV2Sanitizer.Sanitize(item.Result.Score.ToString("N0"));
				string rank = MarkdownV2Sanitizer.Sanitize($"#{item.Result.Rank}");
				
				sb.AppendLine($"• {languageName} \\({framework}\\): {score} req/s {rank}");
			}

			if (validResults.Count >= 2) {
				sb.AppendLine();
				string fastestLang = MarkdownV2Sanitizer.Sanitize(fastest.Language);
				string slowestLang = MarkdownV2Sanitizer.Sanitize(slowest.Language);
				string percentage = MarkdownV2Sanitizer.Sanitize(percentageFaster.ToString("F1"));
				sb.AppendLine($"*{fastestLang}* is *{percentage}%* faster than *{slowestLang}*");
			}

			return sb.ToString();
		}
	}
}
