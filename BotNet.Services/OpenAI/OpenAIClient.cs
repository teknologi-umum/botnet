using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Json;
using BotNet.Services.OpenAI.Models;
using Microsoft.Extensions.Options;
using RG.Ninja;

namespace BotNet.Services.OpenAI {
	public class OpenAIClient(
		HttpClient httpClient,
		IOptions<OpenAIOptions> openAIOptionsAccessor
	) {
		private const string COMPLETION_URL_TEMPLATE = "https://api.openai.com/v1/engines/{0}/completions";
		private const string CHAT_URL = "https://api.openai.com/v1/chat/completions";
		private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new() {
			PropertyNamingPolicy = new SnakeCaseNamingPolicy()
		};
		private readonly HttpClient _httpClient = httpClient;
		private readonly string _apiKey = openAIOptionsAccessor.Value.ApiKey!;

		public async Task<string> AutocompleteAsync(string engine, string prompt, string[]? stop, int maxTokens, double frequencyPenalty, double presencePenalty, double temperature, double topP, CancellationToken cancellationToken) {
			using HttpRequestMessage request = new(HttpMethod.Post, string.Format(COMPLETION_URL_TEMPLATE, engine)) {
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
						TopP = topP,
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
				string? line = await streamReader.ReadLineAsync(cancellationToken);
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

		public async Task<string> ChatAsync(string model, IEnumerable<ChatMessage> messages, int maxTokens, CancellationToken cancellationToken) {
			using HttpRequestMessage request = new(HttpMethod.Post, CHAT_URL) {
				Headers = {
					{ "Authorization", $"Bearer {_apiKey}" },
				},
				Content = JsonContent.Create(
					inputValue: new {
						Model = model,
						MaxTokens = maxTokens,
						Messages = messages
					},
					options: JSON_SERIALIZER_OPTIONS
				)
			};
			using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			CompletionResult? completionResult = await response.Content.ReadFromJsonAsync<CompletionResult>(JSON_SERIALIZER_OPTIONS, cancellationToken);
			if (completionResult == null) return "";
			if (completionResult.Choices.Count == 0) return "";
			return completionResult.Choices[0].Message?.Content!;
		}

		public async IAsyncEnumerable<(string Result, bool Stop)> StreamChatAsync(
			string model,
			IEnumerable<ChatMessage> messages,
			int maxTokens,
			[EnumeratorCancellation] CancellationToken cancellationToken
		) {
			using HttpRequestMessage request = new(HttpMethod.Post, CHAT_URL) {
				Headers = {
					{ "Authorization", $"Bearer {_apiKey}" },
					{ "Accept", "text/event-stream" }
				},
				Content = JsonContent.Create(
					inputValue: new {
						Model = model,
						MaxTokens = maxTokens,
						Messages = messages,
						Stream = true
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
				string? line = await streamReader.ReadLineAsync(cancellationToken);
				if (line == null) break;
				if (line == "") continue;
				if (!line.StartsWith("data: ", out string? json)) break;
				if (json == "[DONE]") break;
				CompletionResult? completionResult = JsonSerializer.Deserialize<CompletionResult>(json, JSON_SERIALIZER_OPTIONS);
				if (completionResult == null) break;
				if (completionResult.Choices.Count == 0) break;
				result.Append(completionResult.Choices[0].Delta!.Content);

				if (completionResult.Choices[0].FinishReason == "stop") {
					yield return (
						Result: result.ToString(),
						Stop: true
					);
					yield break;
				} else {
					yield return (
						Result: result.ToString(),
						Stop: false
					);
				}
			}
		}
	}
}
