using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using BotNet.Services.OpenGraph.Models;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace BotNet.Controllers {
	[Route("opengraph")]
	public class OpenGraphController : Controller {
		private readonly IClusterClient _clusterClient;

		public OpenGraphController(
			IClusterClient clusterClient
		) {
			_clusterClient = clusterClient;
		}

		[HttpGet("image")]
		public async Task<IActionResult> GetPreviewImageAsync(string url) {
			OpenGraphMetadata metadata = await _clusterClient
				.GetGrain<IOpenGraphGrain>(url.Trim().ToLowerInvariant())
				.GetMetadataAsync();
			if (metadata.Image is string previewImageUrl) {
				return RedirectPermanent(previewImageUrl);
			} else {
				return NotFound();
			}
		}
	}
}
