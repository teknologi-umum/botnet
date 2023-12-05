using System.Threading;
using System.Threading.Tasks;

namespace BotNet.Services.Stability.Skills {
	public sealed class ImageVariationBot(
		StabilityClient stabilityClient
	) {
		private readonly StabilityClient _stabilityClient = stabilityClient;

		public async Task<byte[]> ModifyImageAsync(
			byte[] image,
			string prompt,
			CancellationToken cancellationToken
		) {
			return await _stabilityClient.ModifyImageAsync(
				engine: "stable-diffusion-xl-1024-v1-0",
				promptImage: image,
				promptText: prompt,
				cancellationToken: cancellationToken
			);
		}
	}
}
