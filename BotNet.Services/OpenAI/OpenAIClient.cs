using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Json;
using BotNet.Services.OpenAI.Models;
using Microsoft.Extensions.Options;

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

		public Task<string> DavinciAutocompleteAsync(string source, string[] stop, int maxRecursion, CancellationToken cancellationToken) {
			return AutocompleteAsync(DAVINCI_COMPLETIONS_URL, source, stop, maxRecursion, cancellationToken);
		}

		public Task<string> DavinciCodexAutocompleteAsync(string source, string[] stop, int maxRecursion, CancellationToken cancellationToken) {
			return AutocompleteAsync(DAVINCI_CODEX_COMPLETIONS_URL, source, stop, maxRecursion, cancellationToken);
		}

		private async Task<string> AutocompleteAsync(string url, string source, string[]? stop, int maxRecursion, CancellationToken cancellationToken) {
			using HttpRequestMessage request = new(HttpMethod.Post, url) {
				Headers = {
					{ "Authorization", $"Bearer {_apiKey}" }
				},
				Content = JsonContent.Create(
					inputValue: new {
						Prompt = source,
						Temperature = 0.0,
						MaxTokens = 64,
						TopP = 1.0,
						FrequencyPenalty = 0.0,
						PresencePenalty = 0.0,
						Stop = stop
					},
					options: JSON_SERIALIZER_OPTIONS
				)
			};
			using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			CompletionResult completionResult = (await response.Content.ReadFromJsonAsync<CompletionResult>(JSON_SERIALIZER_OPTIONS, cancellationToken))!;
			Choice firstChoice = completionResult.Choices.First();

			if (maxRecursion > 0 && firstChoice.Text!.Length > 0 && firstChoice.FinishReason == "length") {
				return firstChoice.Text! + await AutocompleteAsync(url, source + firstChoice.Text, stop, maxRecursion - 1, cancellationToken);
			} else {
				return firstChoice.Text!;
			}
		}
	}
}
