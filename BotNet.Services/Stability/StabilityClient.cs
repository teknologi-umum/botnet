using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Json;
using BotNet.Services.Stability.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BotNet.Services.Stability {
	public sealed class StabilityClient(
		HttpClient httpClient,
		IOptions<StabilityOptions> optionsAccessor,
		ILogger<StabilityClient> logger
	) {
		private const string TEXT_TO_IMAGE_URL_TEMPLATE = "https://api.stability.ai/v1/generation/{0}/text-to-image";
		private static readonly JsonSerializerOptions SNAKE_CASE_SERIALIZER_OPTIONS = new() {
			PropertyNamingPolicy = new SnakeCaseNamingPolicy()
		};
		private static readonly JsonSerializerOptions CAMEL_CASE_SERIALIZER_OPTIONS = new() {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};
		private readonly HttpClient _httpClient = httpClient;
		private readonly string? _apiKey = optionsAccessor.Value.ApiKey;
		private readonly ILogger<StabilityClient> _logger = logger;

		public async Task<byte[]> GenerateImageAsync(
			string engine,
			string promptText,
			CancellationToken cancellationToken
		) {
			string url = string.Format(TEXT_TO_IMAGE_URL_TEMPLATE, engine);
			using HttpRequestMessage request = new(HttpMethod.Post, url);
			request.Headers.Add("Authorization", $"Bearer {_apiKey}");
			request.Headers.Add("Accept", "application/json");
			request.Content = JsonContent.Create(
				inputValue: new {
					Steps = 40,
					Width = 1024,
					Height = 1024,
					Seed = 0, // random seed
					CfgScale = 5,
					Samples = 1,
					TextPrompts = new[] {
						new {
							Text = promptText,
							Weight = 1
						},
						new {
							Text = "blurry, bad, saturated, high contrast, watermark, signature, label, worst quality, normal quality, low quality, low res, extra digits, cropped, jpeg artifacts, username, error, duplicate, ugly, monochrome, mutation, disgusting, bad anatomy, bad hands, three hands, three legs, bad arms, missing legs, missing arms",
							Weight = -1
						}
					}
				},
				options: SNAKE_CASE_SERIALIZER_OPTIONS
			);
			using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
			if (!response.IsSuccessStatusCode) {
				string error = await response.Content.ReadAsStringAsync(cancellationToken);
				_logger.LogError("Unable to generate image: {0}, HTTP Status Code: {1}", error, (int)response.StatusCode);
				response.EnsureSuccessStatusCode();
			}

			string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

			TextToImageResponse? responseData = JsonSerializer.Deserialize<TextToImageResponse>(responseJson, CAMEL_CASE_SERIALIZER_OPTIONS);

			if (responseData is { Artifacts: [Artifact { FinishReason: "CONTENT_FILTERED" }] }) {
				throw new ContentFilteredException();
			}

			if (responseData is not { Artifacts: [Artifact { FinishReason: "SUCCESS", Base64: var base64 }] }) {
				throw new HttpRequestException();
			}

			return Convert.FromBase64String(base64);
		}
	}
}
