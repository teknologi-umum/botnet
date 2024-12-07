using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI.Skills {
	public class ImageGenerationBot(
		OpenAiClient openAiClient
	) {
		private static readonly SemaphoreSlim Semaphore = new(1, 1);

		public async Task<Uri> GenerateImageAsync(
			string prompt,
			CancellationToken cancellationToken
		) {
			// dall-e-3 endpoint does not allow concurrent requests
			await Semaphore.WaitAsync(cancellationToken);
			try {
				return await openAiClient.GenerateImageAsync(
					model: "dall-e-3",
					prompt: prompt,
					cancellationToken: cancellationToken
				);
			} finally {
				Semaphore.Release();
			}
		}
	}
}
