using Microsoft.AspNetCore.Mvc;

namespace BotNet.Controllers {
	[Route("pse")]
	public class PSEController : Controller {
		[HttpGet]
		public IActionResult Index() {
			return View();
		}
	}
}
