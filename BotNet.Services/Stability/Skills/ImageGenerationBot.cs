using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.Stability.Skills {
	public sealed class ImageGenerationBot(
		StabilityClient stabilityClient
	) {
		private readonly StabilityClient _stabilityClient = stabilityClient;

		public async Task<byte[]> GenerateImageAsync(
			string prompt,
			CancellationToken cancellationToken
		) {
			return await _stabilityClient.GenerateImageAsync(
				engine: "stable-diffusion-xl-1024-v1-0",
				promptText: prompt,
				cancellationToken: cancellationToken
			);
		}
	}
}
