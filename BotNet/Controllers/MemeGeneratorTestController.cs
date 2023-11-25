#if DEBUG
using BotNet.Services.Meme;
using Microsoft.AspNetCore.Mvc;

namespace BotNet.Controllers {
	[Route("meme")]
	public class MemeGeneratorTestController : Controller {
		private readonly MemeGenerator _memeGenerator;

		public MemeGeneratorTestController(
			MemeGenerator memeGenerator
		) {
			_memeGenerator = memeGenerator;
		}

		[Route("ramad")]
		public IActionResult RenderRamad() {
			byte[] memePng = _memeGenerator.CaptionRamad("Melakukan TDD, meski situasi sulit");
			return File(memePng, "image/png", true);
		}
	}
}
#endif
