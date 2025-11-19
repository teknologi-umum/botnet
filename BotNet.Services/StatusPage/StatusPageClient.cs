using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.StatusPage {
	public sealed class StatusPageClient(HttpClient httpClient) {
		private static readonly Dictionary<string, string> ServiceStatusPages = new() {
			// Tech Giants
			{ "GitHub", "https://www.githubstatus.com/api/v2/status.json" },
			{ "Cloudflare", "https://www.cloudflarestatus.com/api/v2/status.json" },
			
			// Social Media
			{ "Discord", "https://discordstatus.com/api/v2/status.json" },
			{ "Reddit", "https://www.redditstatus.com/api/v2/status.json" },
			{ "Twitch", "https://status.twitch.tv/api/v2/status.json" },
			
			// Developer Platforms
			{ "npm", "https://status.npmjs.org/api/v2/status.json" },
			{ "Vercel", "https://www.vercel-status.com/api/v2/status.json" },
			{ "Netlify", "https://www.netlifystatus.com/api/v2/status.json" },
			{ "Atlassian", "https://status.atlassian.com/api/v2/status.json" },
			{ "MongoDB", "https://status.mongodb.com/api/v2/status.json" },
			{ "Supabase", "https://status.supabase.com/api/v2/status.json" },
			{ "Sentry", "https://status.sentry.io/api/v2/status.json" },
			
			// Cloud/Hosting
			{ "DigitalOcean", "https://status.digitalocean.com/api/v2/status.json" },
			{ "Fly.io", "https://status.flyio.net/api/v2/status.json" },
			{ "Dropbox", "https://status.dropbox.com/api/v2/status.json" },
			{ "Oracle Cloud", "https://ocistatus.oraclecloud.com/api/v2/status.json" },
			
			// Communication
			{ "Twilio", "https://status.twilio.com/api/v2/status.json" },
			{ "Zoom", "https://status.zoom.us/api/v2/status.json" },
			
			// Others
			{ "OpenAI", "https://status.openai.com/api/v2/status.json" },
			{ "Shopify", "https://www.shopifystatus.com/api/v2/status.json" },
			{ "Datadog", "https://status.datadoghq.com/api/v2/status.json" },
			{ "New Relic", "https://status.newrelic.com/api/v2/status.json" }
		};

		// Services with custom API formats
		private static readonly Dictionary<string, string> CustomFormatServices = new() {
			{ "Slack", "https://status.slack.com/api/v2.0.0/current" }
		};

		// Meta services - use HTTP availability check (no public status API)
		private static readonly Dictionary<string, string> HttpAvailabilityServices = new() {
			{ "Facebook", "https://www.facebook.com" },
			{ "Instagram", "https://www.instagram.com" },
			{ "WhatsApp", "https://www.whatsapp.com" },
			{ "Threads", "https://www.threads.net" }
		};

		public async Task<List<ServiceStatus>> CheckAllServicesAsync(CancellationToken cancellationToken) {
			List<Task<ServiceStatus>> tasks = new();

			// Add standard format services
			tasks.AddRange(
				ServiceStatusPages.Select(kvp => CheckServiceAsync(kvp.Key, kvp.Value, cancellationToken))
			);

			// Add custom format services
			tasks.AddRange(
				CustomFormatServices.Select(kvp => CheckCustomServiceAsync(kvp.Key, kvp.Value, cancellationToken))
			);

			// Add HTTP availability services
			tasks.AddRange(
				HttpAvailabilityServices.Select(kvp => CheckHttpAvailabilityAsync(kvp.Key, kvp.Value, cancellationToken))
			);

			ServiceStatus[] results = await Task.WhenAll(tasks);
			return results.ToList();
		}

		private async Task<ServiceStatus> CheckCustomServiceAsync(string serviceName, string statusUrl, CancellationToken cancellationToken) {
			try {
				using HttpRequestMessage request = new(HttpMethod.Get, statusUrl);
				request.Headers.Add("User-Agent", "BotNet/1.0");
				
				using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
				
				if (!response.IsSuccessStatusCode) {
					return new ServiceStatus {
						ServiceName = serviceName,
						IsOperational = false,
						Description = "Unable to fetch status"
					};
				}

				string json = await response.Content.ReadAsStringAsync(cancellationToken);

				// Handle Slack's custom format
				if (serviceName == "Slack") {
					SlackStatusResponse? slackResponse = JsonSerializer.Deserialize<SlackStatusResponse>(json);
					
					if (slackResponse == null) {
						return new ServiceStatus {
							ServiceName = serviceName,
							IsOperational = false,
							Description = "Invalid status response"
						};
					}

					bool hasIncidents = slackResponse.ActiveIncidents?.Length > 0;
					bool isOk = slackResponse.Status == "ok";

					return new ServiceStatus {
						ServiceName = serviceName,
						IsOperational = isOk && !hasIncidents,
						Description = isOk && !hasIncidents ? "All Systems Operational" : "Active Incidents"
					};
				}

				return new ServiceStatus {
					ServiceName = serviceName,
					IsOperational = false,
					Description = "Unknown custom format"
				};
			} catch (Exception) {
				return new ServiceStatus {
					ServiceName = serviceName,
					IsOperational = false,
					Description = "Failed to check status"
				};
			}
		}

		private async Task<ServiceStatus> CheckHttpAvailabilityAsync(string serviceName, string url, CancellationToken cancellationToken) {
			try {
				using HttpRequestMessage request = new(HttpMethod.Head, url);
				request.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; BotNet/1.0)");
				
				using CancellationTokenSource timeoutCts = new(TimeSpan.FromSeconds(10));
				using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
				
				using HttpResponseMessage response = await httpClient.SendAsync(request, linkedCts.Token);
				
				// Consider 2xx and 3xx as operational
				bool isOperational = (int)response.StatusCode >= 200 && (int)response.StatusCode < 400;
				
				return new ServiceStatus {
					ServiceName = serviceName,
					IsOperational = isOperational,
					Description = isOperational ? "Service Reachable" : $"HTTP {(int)response.StatusCode}"
				};
			} catch (OperationCanceledException) {
				return new ServiceStatus {
					ServiceName = serviceName,
					IsOperational = false,
					Description = "Request Timeout"
				};
			} catch (HttpRequestException) {
				return new ServiceStatus {
					ServiceName = serviceName,
					IsOperational = false,
					Description = "Connection Failed"
				};
			} catch (Exception) {
				return new ServiceStatus {
					ServiceName = serviceName,
					IsOperational = false,
					Description = "Failed to check status"
				};
			}
		}

		private async Task<ServiceStatus> CheckServiceAsync(string serviceName, string statusUrl, CancellationToken cancellationToken) {
			try {
				using HttpRequestMessage request = new(HttpMethod.Get, statusUrl);
				request.Headers.Add("User-Agent", "BotNet/1.0");
				
				using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
				
				if (!response.IsSuccessStatusCode) {
					return new ServiceStatus {
						ServiceName = serviceName,
						IsOperational = false,
						Description = "Unable to fetch status"
					};
				}

				string json = await response.Content.ReadAsStringAsync(cancellationToken);
				StatusPageResponse? statusPageResponse = JsonSerializer.Deserialize<StatusPageResponse>(json);

				if (statusPageResponse?.Status == null) {
					return new ServiceStatus {
						ServiceName = serviceName,
						IsOperational = false,
						Description = "Invalid status response"
					};
				}

				bool isOperational = statusPageResponse.Status.Indicator switch {
					"none" => true,
					"minor" => false,
					"major" => false,
					"critical" => false,
					_ => true
				};

				return new ServiceStatus {
					ServiceName = serviceName,
					IsOperational = isOperational,
					Description = statusPageResponse.Status.Description
				};
			} catch (Exception) {
				return new ServiceStatus {
					ServiceName = serviceName,
					IsOperational = false,
					Description = "Failed to check status"
				};
			}
		}
	}
}
