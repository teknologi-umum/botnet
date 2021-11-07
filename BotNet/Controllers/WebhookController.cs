using System.Threading;
using System.Threading.Tasks;
using BotNet.Bot;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotNet.Controllers {
	[Route("webhook")]
	public class WebhookController : ControllerBase {
		private readonly ITelegramBotClient _telegramBotClient;
		private readonly string _secretPath;
		private readonly UpdateHandler _updateHandler;

		public WebhookController(
			UpdateHandler updateHandler,
			IOptions<BotOptions> botOptionsAccessor,
			ITelegramBotClient telegramBotClient
		) {
			_updateHandler = updateHandler;
			_secretPath = botOptionsAccessor.Value.AccessToken!.Split(':')[1];
			_telegramBotClient = telegramBotClient;
		}

		[HttpPost("{secretPath}")]
		public async Task<IActionResult> PostAsync(string secretPath, [FromBody] Update update, CancellationToken cancellationToken) {
			if (secretPath != _secretPath) return NotFound();
			await _updateHandler.HandleUpdateAsync(_telegramBotClient, update,  cancellationToken);
			return Ok();
		}
	}
}
