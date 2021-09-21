using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using BotNet.Services.ImageConverter;
using BotNet.Services.OpenGraph.Models;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace BotNet.Controllers {
	[Route("opengraph")]
	public class OpenGraphController : Controller {
		private readonly IClusterClient _clusterClient;
		private readonly IcoToPngConverter _icoToPngConverter;

		public OpenGraphController(
			IClusterClient clusterClient,
			IcoToPngConverter icoToPngConverter
		) {
			_clusterClient = clusterClient;
			_icoToPngConverter = icoToPngConverter;
		}

		[HttpGet("image")]
		public async Task<IActionResult> GetPreviewImageAsync(string url, CancellationToken cancellationToken) {
			string? domain = url.StartsWith("https://") ? url[8..]
				: url.StartsWith("http://") ? url[7..]
				: null;
			if (domain != null) {
				int slashPos = domain.IndexOf('/');
				if (slashPos != -1) domain = domain[..slashPos];
				try {
					string iconUrl = $"https://external-content.duckduckgo.com/ip3/{domain}.ico";
					byte[] iconPng = await _icoToPngConverter.ConvertFromUrlAsync(iconUrl, cancellationToken);
					return File(iconPng, "image/png", true);
				} catch (HttpRequestException) { }
			}
			OpenGraphMetadata? metadata = await _clusterClient
				.GetGrain<IOpenGraphGrain>(url.Trim().ToLowerInvariant())
				.GetMetadataAsync(TimeSpan.FromSeconds(30));
			if (metadata?.Image is string previewImageUrl) {
				if (previewImageUrl.EndsWith(".ico")) {
					try {
						byte[] iconPng = await _icoToPngConverter.ConvertFromUrlAsync(previewImageUrl, cancellationToken);
						return File(iconPng, "image/png", true);
					} catch (HttpRequestException) { }
				}
				return RedirectPermanent(previewImageUrl);
			} else {
				return NotFound();
			}
		}
	}
}
