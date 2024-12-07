using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.Craiyon.Models;

namespace BotNet.Services.Craiyon {
	public class CraiyonClient(
		HttpClient httpClient
	) {
		private const string Url = "https://backend.craiyon.com/generate";
		private static readonly JsonSerializerOptions JsonSerializerOptions = new() {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		public async Task<List<byte[]>> GenerateImagesAsync(string prompt, CancellationToken cancellationToken) {
			using HttpRequestMessage request = new(HttpMethod.Post, Url);
			request.Content = JsonContent.Create(
				inputValue: new {
					Prompt = prompt
				},
				options: JsonSerializerOptions
			);
			using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			ImagesResult? imagesResult = await response.Content.ReadFromJsonAsync<ImagesResult>(JsonSerializerOptions, cancellationToken);

			List<byte[]> images = new();
			if (imagesResult == null) {
				return images;
			}

			foreach (string encodedImage in imagesResult.Images) {
				byte[] image = Convert.FromBase64String(encodedImage.Replace("\\n", ""));
				images.Add(image);
			}
			return images;
		}
	}
}
