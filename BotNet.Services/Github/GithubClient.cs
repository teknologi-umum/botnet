using System;
using System.Collections.Immutable;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Github.Models;
using BotNet.Services.Json;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Github {
	public class GithubClient {
		private const string BASE_URL = "https://api.github.com";
		private readonly HttpClient _httpClient;
		private readonly string _personalAccessToken;
		private readonly JsonSerializerOptions _jsonSerializerOptions;

		public GithubClient(
			HttpClient httpClient,
			IOptions<GithubOptions> githubOptionsAccessor
		) {
			_httpClient = httpClient;
			GithubOptions githubOptions = githubOptionsAccessor.Value;
			_personalAccessToken = githubOptions.PersonalAccessToken ?? throw new InvalidOperationException("Github personal access token not configured. Please add a .NET secret with key 'GithubOptions:PersonalAccessToken' or a Docker secret with key 'GithubOptions__PersonalAccessToken'");
			_jsonSerializerOptions = new JsonSerializerOptions {
				PropertyNamingPolicy = new SnakeCaseNamingPolicy()
			};
		}

		public async Task<ImmutableList<GithubContent>> GetContentAsync(string owner, string repo, string path, CancellationToken cancellationToken) {
			string url = $"{BASE_URL}/repos/{owner}/{repo}/contents/{path.TrimStart('/')}";
			using HttpRequestMessage request = new(HttpMethod.Get, url) {
				Headers = {
					{ "Authorization", $"token {_personalAccessToken}" },
					{ "Accept", "application/vnd.github.v3+json" },
					{ "User-Agent", "TEKNUM" }
				}
			};
			using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			string json = await response.Content.ReadAsStringAsync(cancellationToken);

			return JsonSerializer.Deserialize<ImmutableList<GithubContent>>(json, _jsonSerializerOptions) ?? ImmutableList<GithubContent>.Empty;
		}
	}
}
