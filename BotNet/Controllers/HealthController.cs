using Microsoft.AspNetCore.Mvc;

namespace BotNet.Controllers {
	[Route("")]
	public class HealthController : Controller {
		public IActionResult Index() {
			return Content("https://t.me/teknologi_umum_v2");
		}
	}
}