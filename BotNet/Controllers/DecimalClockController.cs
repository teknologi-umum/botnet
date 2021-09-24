using Microsoft.AspNetCore.Mvc;

namespace BotNet.Controllers {
	[Route("decimalclock")]
	public class DecimalClockController : Controller {
		[Route("svg")]
		[ResponseCache(Duration = 0, NoStore = true)]
		public IActionResult Svg() {
			Response.ContentType = "image/svg+xml";
			return View();
		}
	}
}
