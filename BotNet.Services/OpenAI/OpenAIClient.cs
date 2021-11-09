using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Json;
using BotNet.Services.OpenAI.Models;
using Microsoft.Extensions.Options;
using RG.Ninja;

namespace BotNet.Services.OpenAI {
	public class OpenAIClient {
		private const string DAVINCI_COMPLETIONS_URL = "https://api.openai.com/v1/engines/davinci/completions";
		private const string DAVINCI_CODEX_COMPLETIONS_URL = "https://api.openai.com/v1/engines/davinci-codex/completions";
		private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new() {
			PropertyNamingPolicy = new SnakeCaseNamingPolicy()
		};
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;

		public OpenAIClient(
			HttpClient httpClient,
			IOptions<OpenAIOptions> openAIOptionsAccessor
		) {
			_httpClient = httpClient;
			_apiKey = openAIOptionsAccessor.Value.ApiKey!;
		}

		public Task<string> DavinciAutocompleteAsync(string prompt, string[] stop, int maxTokens, double frequencyPenalty, double presencePenalty, double temperature, CancellationToken cancellationToken) {
			return AutocompleteAsync(DAVINCI_COMPLETIONS_URL, prompt, stop, maxTokens, frequencyPenalty, presencePenalty, temperature, cancellationToken);
		}

		public Task<string> DavinciCodexAutocompleteAsync(string prompt, string[] stop, int maxTokens, double frequencyPenalty, double presencePenalty, double temperature, CancellationToken cancellationToken) {
			return AutocompleteAsync(DAVINCI_CODEX_COMPLETIONS_URL, prompt, stop, maxTokens, frequencyPenalty, presencePenalty, temperature, cancellationToken);
		}

		private async Task<string> AutocompleteAsync(string url, string prompt, string[]? stop, int maxTokens, double frequencyPenalty, double presencePenalty, double temperature, CancellationToken cancellationToken) {
			using HttpRequestMessage request = new(HttpMethod.Post, url) {
				Headers = {
					{ "Authorization", $"Bearer {_apiKey}" },
					{ "Accept", "text/event-stream" }
				},
				Content = JsonContent.Create(
					inputValue: new {
						Prompt = prompt,
						Temperature = temperature,
						MaxTokens = maxTokens,
						Stream = true,
						TopP = 1.0,
						FrequencyPenalty = frequencyPenalty,
						PresencePenalty = presencePenalty,
						Stop = stop
					},
					options: JSON_SERIALIZER_OPTIONS
				)
			};
			using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			StringBuilder result = new();
			using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			using StreamReader streamReader = new(stream);
			while (!streamReader.EndOfStream) {
				string? line = await streamReader.ReadLineAsync();
				if (line == null) break;
				if (line == "") continue;
				if (!line.StartsWith("data: ", out string? json)) break;
				if (json == "[DONE]") break;
				CompletionResult? completionResult = JsonSerializer.Deserialize<CompletionResult>(json, JSON_SERIALIZER_OPTIONS);
				if (completionResult == null) break;
				if (completionResult.Choices.Count == 0) break;
				result.Append(completionResult.Choices[0].Text);
				if (completionResult.Choices[0].FinishReason == "stop") break;
			}

			return result.ToString();
		}
	}
}
