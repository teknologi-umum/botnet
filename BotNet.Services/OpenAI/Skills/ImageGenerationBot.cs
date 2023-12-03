using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI.Skills {
	public class ImageGenerationBot(
		OpenAIClient openAIClient
	) {
		private readonly OpenAIClient _openAIClient = openAIClient;
		private static readonly SemaphoreSlim SEMAPHORE = new(1, 1);

		public async Task<Uri> GenerateImageAsync(
			string prompt,
			CancellationToken cancellationToken
		) {
			// dall-e-3 endpoint does not allow concurrent requests
			await SEMAPHORE.WaitAsync(cancellationToken);
			try {
				return await _openAIClient.GenerateImageAsync(
					model: "dall-e-3",
					prompt: prompt,
					cancellationToken: cancellationToken
				);
			} finally {
				SEMAPHORE.Release();
			}
		}
	}
}
