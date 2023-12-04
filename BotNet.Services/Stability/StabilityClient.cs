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
		private const string IMAGE_TO_IMAGE_URL_TEMPLATE = "https://api.stability.ai/v1/generation/{0}/image-to-image";

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

			if (responseData is not { Artifacts: [Artifact { FinishReason: "SUCCESS", Base64: var base64 }] }) {
				throw new HttpRequestException();
			}

			return Convert.FromBase64String(base64);
		}

		public async Task<byte[]> ModifyImageAsync(
			string engine,
			byte[] promptImage,
			string promptText,
			CancellationToken cancellationToken
		) {
			string url = string.Format(IMAGE_TO_IMAGE_URL_TEMPLATE, engine);
			using HttpRequestMessage request = new(HttpMethod.Post, url);
			request.Headers.Add("Authorization", $"Bearer {_apiKey}");
			request.Headers.Add("Accept", "application/json");
			using MultipartFormDataContent formData = new();
			using ByteArrayContent promptImageContent = new(promptImage);
			formData.Add(
				content: promptImageContent,
				name: "init_image",
				fileName: "init_image.png"
			);
			using StringContent initImageMode = new("IMAGE_STRENGTH");
			using StringContent imageStrength = new("0.35");
			using StringContent steps = new("40");
			using StringContent width = new("1024");
			using StringContent height = new("1024");
			using StringContent seed = new("0");
			using StringContent cfgScale = new("5");
			using StringContent samples = new("1");
			using StringContent textPrompts0Text = new(promptText);
			using StringContent textPrompts0Weight = new("1");
			using StringContent textPrompts1Text = new("blurry, bad, saturated, high contrast, watermark, signature, label, worst quality, normal quality, low quality, low res, extra digits, cropped, jpeg artifacts, username, error, duplicate, ugly, monochrome, mutation, disgusting, bad anatomy, bad hands, three hands, three legs, bad arms, missing legs, missing arms");
			using StringContent textPrompts1Weight = new("-1");
			formData.Add(initImageMode, "init_image_mode");
			formData.Add(imageStrength, "image_strength");
			formData.Add(steps, "steps");
			formData.Add(width, "width");
			formData.Add(height, "height");
			formData.Add(seed, "seed");
			formData.Add(cfgScale, "cfg_scale");
			formData.Add(samples, "samples");
			formData.Add(textPrompts0Text, "text_prompts[0][text]");
			formData.Add(textPrompts0Weight, "text_prompts[0][weight]");
			formData.Add(textPrompts1Text, "text_prompts[1][text]");
			formData.Add(textPrompts1Weight, "text_prompts[1][weight]");
			request.Content = formData;
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
