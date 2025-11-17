using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace BotNet.Services.TechEmpower {
	public sealed class TechEmpowerScraper(
		HttpClient httpClient
	) {
		private const string BenchmarkUrl = "https://www.techempower.com/benchmarks/#section=data-r22&test=composite";

		public async Task<BenchmarkResult[]> GetCompositeScoresAsync(CancellationToken cancellationToken) {
			using HttpRequestMessage httpRequest = new(HttpMethod.Get, BenchmarkUrl);
			using HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
			httpResponse.EnsureSuccessStatusCode();

			string html = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

			IBrowsingContext browsingContext = BrowsingContext.New(Configuration.Default);
			IDocument document = await browsingContext.OpenAsync(req => req.Content(html), cancellationToken);

			// The TechEmpower benchmark page uses a table structure
			// We need to find the results table and extract framework, language, and scores
			IHtmlTableElement? resultsTable = document.QuerySelector<IHtmlTableElement>("table.results");
			
			if (resultsTable is null) {
				throw new InvalidOperationException("Could not find benchmark results table on TechEmpower page.");
			}

			List<BenchmarkResult> results = new();
			IHtmlCollection<IHtmlTableRowElement> rows = resultsTable.Rows;
			
			// Skip header row (index 0)
			for (int i = 1; i < rows.Length; i++) {
				IHtmlTableRowElement row = rows[i];
				IHtmlCollection<IHtmlTableCellElement> cells = row.Cells;
				
				if (cells.Length < 4) continue;

				// Typical structure: Rank | Framework | Language | Score
				// Adjust indices based on actual table structure
				string rankText = cells[0].TextContent.Trim();
				string framework = cells[1].TextContent.Trim();
				string language = cells[2].TextContent.Trim();
				string scoreText = cells[3].TextContent.Trim();

				if (!int.TryParse(rankText, out int rank)) continue;
				if (!double.TryParse(scoreText.Replace(",", ""), out double score)) continue;

				results.Add(new BenchmarkResult {
					Rank = rank,
					Framework = framework,
					Language = language,
					Score = score
				});
			}

			return results.ToArray();
		}

		public BenchmarkResult? GetBestResultForLanguage(BenchmarkResult[] results, string languageName) {
			// Find the best (lowest rank / highest score) result for a given language
			// Case-insensitive match
			return results
				.Where(r => r.Language.Equals(languageName, StringComparison.OrdinalIgnoreCase))
				.OrderBy(r => r.Rank)
				.FirstOrDefault();
		}
	}
}
