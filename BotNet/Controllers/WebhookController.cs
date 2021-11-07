using System.Threading;
using System.Threading.Tasks;
using BotNet.Bot;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotNet.Controllers {
	public class WebhookController : ControllerBase {
		private readonly ITelegramBotClient _telegramBotClient;
		private readonly string _botToken;
		private readonly UpdateHandler _updateHandler;

		public WebhookController(
			UpdateHandler updateHandler,
			IOptions<BotOptions> botOptionsAccessor,
			ITelegramBotClient telegramBotClient
		) {
			_updateHandler = updateHandler;
			_botToken = botOptionsAccessor.Value.AccessToken!;
			_telegramBotClient = telegramBotClient;
		}

		[HttpPost("{token}")]
		public async Task<IActionResult> PostAsync(string token, [FromBody] Update update, CancellationToken cancellationToken) {
			if (token != _botToken) return NotFound();
			await _updateHandler.HandleUpdateAsync(_telegramBotClient, update,  cancellationToken);
			return Ok();
		}
	}
}
