using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.TechEmpower {
	public sealed class TechEmpowerScraper(
		HttpClient httpClient
	) {
		private const string BenchmarkUrl = "https://www.techempower.com/benchmarks/results/round23/ph.json";

		public async Task<BenchmarkResult[]> GetCompositeScoresAsync(CancellationToken cancellationToken) {
			using HttpRequestMessage httpRequest = new(HttpMethod.Get, BenchmarkUrl);
			using HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
			httpResponse.EnsureSuccessStatusCode();

			TechEmpowerBenchmarkData? data = await httpResponse.Content.ReadFromJsonAsync<TechEmpowerBenchmarkData>(cancellationToken);
			
			if (data is null) {
				throw new InvalidOperationException("Could not parse TechEmpower benchmark JSON data.");
			}

			// Build a mapping of framework name to metadata
			Dictionary<string, FrameworkMetadata> metadataMap = data.TestMetadata
				.ToDictionary(m => m.Name, m => m);

			List<BenchmarkResult> results = new();

			// Use plaintext test as it's a common baseline for raw performance
			if (data.RawData?.Plaintext is null) {
				throw new InvalidOperationException("Plaintext test data not found in benchmark results.");
			}

			foreach (KeyValuePair<string, List<TestResult>?> kvp in data.RawData.Plaintext) {
				string frameworkName = kvp.Key;
				List<TestResult>? testResults = kvp.Value;

				if (testResults is null || testResults.Count == 0) continue;
				if (!metadataMap.TryGetValue(frameworkName, out FrameworkMetadata? metadata)) continue;

				// Use the highest concurrency level result (last in array) for best performance
				TestResult bestResult = testResults[testResults.Count - 1];
				
				// Calculate requests per second: totalRequests / duration (15 seconds per test)
				double requestsPerSecond = bestResult.TotalRequests / 15.0;

				results.Add(new BenchmarkResult {
					Framework = metadata.DisplayName ?? frameworkName,
					Language = metadata.Language ?? "Unknown",
					Score = requestsPerSecond,
					Rank = 0 // Will be set after sorting
				});
			}

			// Sort by score descending and assign ranks
			BenchmarkResult[] sortedResults = results
				.OrderByDescending(r => r.Score)
				.ToArray();

			for (int i = 0; i < sortedResults.Length; i++) {
				sortedResults[i] = sortedResults[i] with { Rank = i + 1 };
			}

			return sortedResults;
		}

		public BenchmarkResult? GetBestResultForLanguage(BenchmarkResult[] results, string languageName) {
			// Find the best (lowest rank / highest score) result for a given language
			// Case-insensitive match
			return results
				.Where(r => r.Language.Equals(languageName, StringComparison.OrdinalIgnoreCase))
				.OrderBy(r => r.Rank)
				.FirstOrDefault();
		}

		public BenchmarkResult[] GetTopResultsForLanguage(BenchmarkResult[] results, string languageName, int count = 3) {
			// Find the top N results for a given language
			// Case-insensitive match
			return results
				.Where(r => r.Language.Equals(languageName, StringComparison.OrdinalIgnoreCase))
				.OrderBy(r => r.Rank)
				.Take(count)
				.ToArray();
		}

		public BenchmarkResult? GetResultByFrameworkName(BenchmarkResult[] results, string frameworkName) {
			// Find exact framework/stack by name
			// Case-insensitive match
			return results
				.FirstOrDefault(r => r.Framework.Equals(frameworkName, StringComparison.OrdinalIgnoreCase));
		}

		public BenchmarkResult[] GetResultsByFrameworkNamePrefix(BenchmarkResult[] results, string frameworkNamePrefix, int count = 3) {
			// Find frameworks that start with the given prefix
			// Case-insensitive match, ordered by rank
			return results
				.Where(r => r.Framework.StartsWith(frameworkNamePrefix, StringComparison.OrdinalIgnoreCase))
				.OrderBy(r => r.Rank)
				.Take(count)
				.ToArray();
		}

		public bool IsLanguageName(BenchmarkResult[] results, string query) {
			// Check if the query matches any language name
			return results.Any(r => r.Language.Equals(query, StringComparison.OrdinalIgnoreCase));
		}
	}

	internal sealed record TechEmpowerBenchmarkData {
		public FrameworkMetadata[] TestMetadata { get; init; } = Array.Empty<FrameworkMetadata>();
		public RawDataContainer? RawData { get; init; }
	}

	internal sealed record RawDataContainer {
		public Dictionary<string, List<TestResult>?>? Plaintext { get; init; }
	}

	internal sealed record FrameworkMetadata {
		public string Name { get; init; } = null!;
		public string? Language { get; init; }
		public string? Framework { get; init; }
		public string? DisplayName { get; init; }
	}

	internal sealed record TestResult {
		public long TotalRequests { get; init; }
	}
}
