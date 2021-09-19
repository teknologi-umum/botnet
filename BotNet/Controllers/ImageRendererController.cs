using System;
using BotNet.Services.ColorCard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BotNet.Controllers {
	[Route("renderer")]
	public class ImageRendererController : Controller {
		private readonly ColorCardRenderer _colorCardRenderer;
		private readonly ILogger<ImageRendererController> _logger;

		public ImageRendererController(
			ColorCardRenderer colorCardRenderer,
			ILogger<ImageRendererController> logger
		) {
			_colorCardRenderer = colorCardRenderer;
			_logger = logger;
		}

		[HttpGet("color")]
		public IActionResult RenderColorCard(string name) {
			try {
				byte[] colorCardPng = _colorCardRenderer.RenderColorCard(name);
				return File(colorCardPng, "image/png", true);
			} catch (Exception exc) {
				_logger.LogError(exc, "Cannot render color card.");
				return NotFound();
			}
		}

		[HttpGet("color/preview")]
		public IActionResult RenderColorCardPreview(string name) {
			try {
				byte[] colorCardPng = _colorCardRenderer.RenderColorCardPreview(name);
				return File(colorCardPng, "image/png", true);
			} catch (Exception exc) {
				_logger.LogError(exc, "Cannot render color card.");
				return NotFound();
			}
		}
	}
}
