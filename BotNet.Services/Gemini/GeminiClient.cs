﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Gemini.Models;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Gemini {
	public class GeminiClient(
		HttpClient httpClient,
		IOptions<GeminiOptions> geminiOptionsAccessor
	) {
		private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-05-20:generateContent";
		private readonly string _apiKey = geminiOptionsAccessor.Value.ApiKey!;

		public async Task<string> ChatAsync(IEnumerable<Content> messages, int maxTokens, CancellationToken cancellationToken) {
			GeminiRequest geminiRequest = new(
				Contents: messages.ToImmutableList(),
				SafetySettings: [
					new SafetySettings("HARM_CATEGORY_HARASSMENT", "BLOCK_NONE"),
					new SafetySettings("HARM_CATEGORY_HATE_SPEECH", "BLOCK_NONE"),
					new SafetySettings("HARM_CATEGORY_SEXUALLY_EXPLICIT", "BLOCK_NONE"),
					new SafetySettings("HARM_CATEGORY_DANGEROUS_CONTENT", "BLOCK_NONE")
				],
				GenerationConfig: new(
					MaxOutputTokens: maxTokens
				)
			);
			using HttpRequestMessage request = new(HttpMethod.Post, $"{BaseUrl}?key={_apiKey}");
			request.Headers.Add("Accept", "application/json");
			request.Content = JsonContent.Create(
				inputValue: geminiRequest
			);
			using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
			string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
			response.EnsureSuccessStatusCode();

			GeminiResponse? geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);
			if (geminiResponse == null) return "";
			if (geminiResponse.Candidates == null) return "";
			if (geminiResponse.Candidates.Count == 0) return "";
			Content? content = geminiResponse.Candidates[0].Content;
			if (content == null) return "";
			if (content.Parts == null) return "";
			if (content.Parts.Count == 0) return "";
			return content.Parts[0].Text ?? "";
		}
	}
}
