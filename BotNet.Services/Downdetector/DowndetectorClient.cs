using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.Downdetector {
	public sealed class DowndetectorClient(HttpClient httpClient) {
		private static readonly Dictionary<string, string> ServiceUrls = new() {
			{ "Google", "https://downdetector.com/status/google/" },
			{ "Facebook", "https://downdetector.com/status/facebook/" },
			{ "Instagram", "https://downdetector.com/status/instagram/" },
			{ "WhatsApp", "https://downdetector.com/status/whatsapp/" },
			{ "YouTube", "https://downdetector.com/status/youtube/" },
			{ "Twitter/X", "https://downdetector.com/status/twitter/" },
			{ "Amazon", "https://downdetector.com/status/amazon/" },
			{ "Microsoft", "https://downdetector.com/status/microsoft/" },
			{ "Gmail", "https://downdetector.com/status/gmail/" },
			{ "Outlook", "https://downdetector.com/status/outlook/" }
		};

		public async Task<List<DowndetectorServiceStatus>> CheckServicesAsync(CancellationToken cancellationToken) {
			List<Task<DowndetectorServiceStatus>> tasks = ServiceUrls
				.Select(kvp => CheckServiceAsync(kvp.Key, kvp.Value, cancellationToken))
				.ToList();

			DowndetectorServiceStatus[] results = await Task.WhenAll(tasks);
			return results.ToList();
		}

		private async Task<DowndetectorServiceStatus> CheckServiceAsync(
			string serviceName,
			string url,
			CancellationToken cancellationToken
		) {
			try {
				using HttpRequestMessage request = new(HttpMethod.Get, url);
				request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
				
				using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
				
				if (!response.IsSuccessStatusCode) {
					return new DowndetectorServiceStatus {
						ServiceName = serviceName,
						HasIssues = null,
						Description = "Unable to fetch status"
					};
				}

				string html = await response.Content.ReadAsStringAsync(cancellationToken);
				
				// Look for indicators of service issues in the HTML
				bool hasProblems = DetectIssues(html);
				
				return new DowndetectorServiceStatus {
					ServiceName = serviceName,
					HasIssues = hasProblems,
					Description = hasProblems ? "Possible issues detected" : "No problems detected"
				};
			} catch (Exception) {
				return new DowndetectorServiceStatus {
					ServiceName = serviceName,
					HasIssues = null,
					Description = "Failed to check status"
				};
			}
		}

		private static bool DetectIssues(string html) {
			// Look for common patterns that indicate issues
			// Downdetector shows "problems" or "issues" prominently when detected
			
			// Check for problem indicators in meta tags or prominent text
			if (Regex.IsMatch(html, @"problems?\s+at\s+\w+", RegexOptions.IgnoreCase)) {
				return true;
			}
			
			// Check for "user reports" or "reports" in higher numbers (indicates issues)
			Match reportsMatch = Regex.Match(html, @"(\d{3,})\s+reports?", RegexOptions.IgnoreCase);
			if (reportsMatch.Success && int.TryParse(reportsMatch.Groups[1].Value, out int reportCount)) {
				// If there are more than 100 reports, likely there's an issue
				if (reportCount > 100) {
					return true;
				}
			}
			
			// Check for status indicators
			if (html.Contains("experiencing problems", StringComparison.OrdinalIgnoreCase) ||
			    html.Contains("having issues", StringComparison.OrdinalIgnoreCase) ||
			    html.Contains("outage", StringComparison.OrdinalIgnoreCase)) {
				return true;
			}
			
			// No clear indicators of issues
			return false;
		}
	}
}
