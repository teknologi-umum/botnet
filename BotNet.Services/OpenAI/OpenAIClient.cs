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
		private const string DAVINCI_COMPLETIONS_URL = "https://api.openai.com/v1/engines/davinci-codex/completions";
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

		public async Task<string> DavinciAutocompleteAsync(string source, string[] stop, CancellationToken cancellationToken) {
			using HttpRequestMessage request = new(HttpMethod.Post, DAVINCI_COMPLETIONS_URL) {
				Headers = {
					{ "Authorization", $"Bearer {_apiKey}" }
				},
				Content = JsonContent.Create(
					inputValue: new {
						Prompt = source,
						Temperature = 0,
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

			return firstChoice.Text!;
		}
	}
}
