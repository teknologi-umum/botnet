using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Craiyon.Models;

namespace BotNet.Services.Craiyon {
	public class CraiyonClient {
		private const string URL = "https://backend.craiyon.com/generate";
		private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new() {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};
		private readonly HttpClient _httpClient;

		public CraiyonClient(
			HttpClient httpClient
		) {
			_httpClient = httpClient;
		}

		public async Task<List<byte[]>> GenerateImagesAsync(string prompt, CancellationToken cancellationToken) {
			using HttpRequestMessage request = new(HttpMethod.Post, URL) {
				Content = JsonContent.Create(
					inputValue: new {
						Prompt = prompt
					},
					options: JSON_SERIALIZER_OPTIONS
				)
			};
			using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			ImagesResult? imagesResult = await response.Content.ReadFromJsonAsync<ImagesResult>(JSON_SERIALIZER_OPTIONS, cancellationToken);

			List<byte[]> images = new();
			if (imagesResult != null) {
				foreach (string encodedImage in imagesResult.Images) {
					byte[] image = Convert.FromBase64String(encodedImage.Replace("\\n", ""));
					images.Add(image);
				}
			}
			return images;
		}
	}
}
