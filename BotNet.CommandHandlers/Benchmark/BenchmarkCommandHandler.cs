using System.IO;
using BotNet.Commands.Benchmark;
using BotNet.Services.MarkdownV2;
using BotNet.Services.RateLimit;
using BotNet.Services.TechEmpower;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Benchmark {
	public sealed class BenchmarkCommandHandler(
		ITelegramBotClient telegramBotClient,
		TechEmpowerScraper techEmpowerScraper,
		BenchmarkChartRenderer benchmarkChartRenderer,
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

				// Get top 3 results for each requested language/framework
				List<BenchmarkResult> selectedResults = new();
				List<string> notFound = new();

				foreach (string query in command.Languages) {
					// Priority 1: Check if it's a language name
					if (techEmpowerScraper.IsLanguageName(allResults, query)) {
						// Get top 3 for the language
						BenchmarkResult[] topResults = techEmpowerScraper.GetTopResultsForLanguage(allResults, query, 3);
						if (topResults.Length > 0) {
							selectedResults.AddRange(topResults);
							continue;
						}
					}

					// Priority 2: Try partial framework name match (starts with)
					BenchmarkResult[] partialMatches = techEmpowerScraper.GetResultsByFrameworkNamePrefix(allResults, query);
					if (partialMatches.Length > 0) {
						selectedResults.AddRange(partialMatches);
						continue;
					}

					// Priority 3: Try exact framework name match
					BenchmarkResult? exactMatch = techEmpowerScraper.GetResultByFrameworkName(allResults, query);
					if (exactMatch is not null) {
						selectedResults.Add(exactMatch);
						continue;
					}

					// Not found
					notFound.Add(query);
				}

				if (notFound.Count > 0) {
					string notFoundList = string.Join(", ", notFound.Select(MarkdownV2Sanitizer.Sanitize));
					await telegramBotClient.SendMessage(
						chatId: command.Chat.Id,
						text: $"Could not find benchmark results for: {notFoundList}",
						parseMode: ParseMode.MarkdownV2,
						replyParameters: new ReplyParameters {
							MessageId = command.MessageId
						},
						cancellationToken: cancellationToken
					);
					return;
				}

				// Generate chart visualization
				byte[] chartImage = benchmarkChartRenderer.RenderBenchmarkChart(selectedResults.ToArray());

				// Format text response
				string response = FormatBenchmarkResponse(selectedResults);

				// Send chart image with text caption
				using MemoryStream stream = new(chartImage);
				await telegramBotClient.SendPhoto(
					chatId: command.Chat.Id,
					photo: new InputFileStream(stream, "benchmark.png"),
					caption: response,
					parseMode: ParseMode.MarkdownV2,
					replyParameters: new ReplyParameters {
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

		private static string FormatBenchmarkResponse(List<BenchmarkResult> results) {
			if (results.Count == 0) {
				return "No results found.";
			}

			// Sort by score descending
			List<BenchmarkResult> sortedResults = results
				.OrderByDescending(r => r.Score)
				.ToList();

			System.Text.StringBuilder sb = new();
			sb.AppendLine("*TechEmpower Benchmark Results:*");
			sb.AppendLine();

			// Display all results
			foreach (BenchmarkResult result in sortedResults) {
				string language = MarkdownV2Sanitizer.Sanitize(result.Language);
				string framework = MarkdownV2Sanitizer.Sanitize(result.Framework);
				string score = MarkdownV2Sanitizer.Sanitize(result.Score.ToString("N0"));
				string rank = MarkdownV2Sanitizer.Sanitize($"#{result.Rank}");
				
				sb.AppendLine($"• {language} \\({framework}\\): {score} req/s {rank}");
			}

			// If only 1 result, no comparison needed
			if (sortedResults.Count == 1) {
				sb.AppendLine();
				sb.AppendLine("[View full results](https://www\\.techempower\\.com/benchmarks/)");
				return sb.ToString();
			}

			// If 2+ results, compare fastest against each other
			sb.AppendLine();
			BenchmarkResult fastest = sortedResults[0];
			
			for (int i = 1; i < sortedResults.Count; i++) {
				BenchmarkResult current = sortedResults[i];
				double percentageFaster = ((fastest.Score - current.Score) / current.Score) * 100;
				
				string fastestFramework = MarkdownV2Sanitizer.Sanitize(fastest.Framework);
				string currentFramework = MarkdownV2Sanitizer.Sanitize(current.Framework);
				string percentage = MarkdownV2Sanitizer.Sanitize(percentageFaster.ToString("F1"));
				
				sb.AppendLine($"*{fastestFramework}* is *{percentage}%* faster than *{currentFramework}*");
			}

			sb.AppendLine();
			sb.AppendLine("[View full results](https://www\\.techempower\\.com/benchmarks/)");

			return sb.ToString();
		}
	}
}
