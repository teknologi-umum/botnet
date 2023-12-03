using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.OpenAI.Skills {
	public class ImageGenerationBot(
		OpenAIClient openAIClient
	) {
		private readonly OpenAIClient _openAIClient = openAIClient;

		public Task<Uri> GenerateImageAsync(
			string prompt,
			CancellationToken cancellationToken
		) => _openAIClient.GenerateImageAsync(
			model: "dall-e-3",
			prompt: prompt,
			cancellationToken: cancellationToken
		);
	}
}
