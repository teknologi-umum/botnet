using System;
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RG.Ninja;

namespace BotNet.Services.OpenAI {
	public class OpenAiClient(
		HttpClient httpClient,
		IOptions<OpenAiOptions> openAiOptionsAccessor,
		ILogger<OpenAiClient> logger
	) {
		private const string CompletionUrlTemplate = "https://api.openai.com/v1/engines/{0}/completions";
		private const string ChatUrl = "https://api.openai.com/v1/chat/completions";
		private const string ImageGenerationUrl = "https://api.openai.com/v1/images/generations";
		private static readonly JsonSerializerOptions JsonSerializerOptions = new() {
			PropertyNamingPolicy = new SnakeCaseNamingPolicy()
		};

		private readonly string _apiKey = openAiOptionsAccessor.Value.ApiKey!;

		public async Task<string> AutocompleteAsync(string engine, string prompt, string[]? stop, int maxTokens, double frequencyPenalty, double presencePenalty, double temperature, double topP, CancellationToken cancellationToken) {
			using HttpRequestMessage request = new(HttpMethod.Post, string.Format(CompletionUrlTemplate, engine));
			request.Headers.Add("Authorization", $"Bearer {_apiKey}");
			request.Headers.Add("Accept", "text/event-stream");
			request.Content = JsonContent.Create(
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
				options: JsonSerializerOptions
			);
			using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			StringBuilder result = new();
			await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			using StreamReader streamReader = new(stream);
			while (!streamReader.EndOfStream) {
				string? line = await streamReader.ReadLineAsync(cancellationToken);
				if (line == null) break;
				if (line == "") continue;
				if (!line.StartsWith("data: ", out string? json)) break;
				if (json == "[DONE]") break;
				CompletionResult? completionResult = JsonSerializer.Deserialize<CompletionResult>(json, JsonSerializerOptions);
				if (completionResult == null) break;
				if (completionResult.Choices.Count == 0) break;
				result.Append(completionResult.Choices[0].Text);
				if (completionResult.Choices[0].FinishReason == "stop") break;
			}

			return result.ToString();
		}

		public async Task<string> ChatAsync(string model, IEnumerable<ChatMessage> messages, int maxTokens, CancellationToken cancellationToken) {
			using HttpRequestMessage request = new(HttpMethod.Post, ChatUrl);
			request.Headers.Add("Authorization", $"Bearer {_apiKey}");
			request.Content = JsonContent.Create(
				inputValue: new {
					Model = model,
					MaxTokens = maxTokens,
					Messages = messages
				},
				options: JsonSerializerOptions
			);
			using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			CompletionResult? completionResult = await response.Content.ReadFromJsonAsync<CompletionResult>(JsonSerializerOptions, cancellationToken);
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
			using HttpRequestMessage request = new(HttpMethod.Post, ChatUrl);
			request.Headers.Add("Authorization", $"Bearer {_apiKey}");
			request.Headers.Add("Accept", "text/event-stream");
			request.Content = JsonContent.Create(
				inputValue: new {
					Model = model,
					MaxTokens = maxTokens,
					Messages = messages,
					Stream = true
				},
				options: JsonSerializerOptions
			);
			using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
			if (!response.IsSuccessStatusCode) {
				string errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
				logger.LogError(errorMessage);
				response.EnsureSuccessStatusCode();
			}

			StringBuilder result = new();
			await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			using StreamReader streamReader = new(stream);

			while (!streamReader.EndOfStream) {
				string? line = await streamReader.ReadLineAsync(cancellationToken);

				if (line == null) {
					yield return (
						Result: result.ToString(),
						Stop: true
					);
					yield break;
				}

				if (line == "") continue;
				if (!line.StartsWith("data: ", out string? json)) break;

				if (json == "[DONE]") {
					yield return (
						Result: result.ToString(),
						Stop: true
					);
					yield break;
				}

				CompletionResult? completionResult = JsonSerializer.Deserialize<CompletionResult>(json, JsonSerializerOptions);

				if (completionResult == null || completionResult.Choices.Count == 0) {
					yield return (
						Result: result.ToString(),
						Stop: true
					);
					yield break;
				}

				result.Append(completionResult.Choices[0].Delta!.Content);

				if (completionResult.Choices[0].FinishReason == "stop") {
					yield return (
						Result: result.ToString(),
						Stop: true
					);
					yield break;
				}

				yield return (
					Result: result.ToString(),
					Stop: false
				);
			}
		}

		public async Task<Uri> GenerateImageAsync(string model, string prompt, CancellationToken cancellationToken) {
			using HttpRequestMessage request = new(HttpMethod.Post, ImageGenerationUrl);
			request.Headers.Add("Authorization", $"Bearer {_apiKey}");
			request.Content = JsonContent.Create(
				inputValue: new {
					Model = model,
					Prompt = prompt,
					N = 1,
					Size = "1024x1024"
				},
				options: JsonSerializerOptions
			);
			using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			ImageGenerationResult? imageGenerationResult = await response.Content.ReadFromJsonAsync<ImageGenerationResult>(JsonSerializerOptions, cancellationToken);
			return new(imageGenerationResult!.Data[0].Url);
		}
	}
}
