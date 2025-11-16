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
			{ "AWS", "https://status.aws.amazon.com/data.json" },
			{ "Azure", "https://status.azure.com/api/v2/status.json" },
			{ "Cloudflare", "https://www.cloudflarestatus.com/api/v2/status.json" },
			
			// Social Media
			{ "Discord", "https://discordstatus.com/api/v2/status.json" },
			{ "Reddit", "https://www.redditstatus.com/api/v2/status.json" },
			{ "Twitch", "https://status.twitch.tv/api/v2/status.json" },
			
			// Developer Platforms
			{ "npm", "https://status.npmjs.org/api/v2/status.json" },
			{ "Docker Hub", "https://status.docker.com/api/v2/status.json" },
			{ "GitLab", "https://status.gitlab.com/api/v2/status.json" },
			{ "Vercel", "https://www.vercel-status.com/api/v2/status.json" },
			{ "Netlify", "https://www.netlifystatus.com/api/v2/status.json" },
			{ "Atlassian", "https://status.atlassian.com/api/v2/status.json" },
			{ "MongoDB", "https://status.mongodb.com/api/v2/status.json" },
			{ "Supabase", "https://status.supabase.com/api/v2/status.json" },
			{ "Sentry", "https://status.sentry.io/api/v2/status.json" },
			
			// Cloud/Hosting
			{ "DigitalOcean", "https://status.digitalocean.com/api/v2/status.json" },
			{ "Heroku", "https://status.heroku.com/api/v2/status.json" },
			{ "Fly.io", "https://status.flyio.net/api/v2/status.json" },
			{ "Dropbox", "https://status.dropbox.com/api/v2/status.json" },
			
			// Communication
			{ "Slack", "https://status.slack.com/api/v2/status.json" },
			{ "Twilio", "https://status.twilio.com/api/v2/status.json" },
			{ "Zoom", "https://status.zoom.us/api/v2/status.json" },
			{ "Zendesk", "https://status.zendesk.com/api/v2/status.json" },
			
			// Others
			{ "Stripe", "https://status.stripe.com/api/v2/status.json" },
			{ "OpenAI", "https://status.openai.com/api/v2/status.json" },
			{ "Shopify", "https://www.shopifystatus.com/api/v2/status.json" },
			{ "Datadog", "https://status.datadoghq.com/api/v2/status.json" }
		};

		public async Task<List<ServiceStatus>> CheckAllServicesAsync(CancellationToken cancellationToken) {
			List<Task<ServiceStatus>> tasks = ServiceStatusPages
				.Select(kvp => CheckServiceAsync(kvp.Key, kvp.Value, cancellationToken))
				.ToList();

			ServiceStatus[] results = await Task.WhenAll(tasks);
			return results.ToList();
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
