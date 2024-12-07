using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotNet.Services.BotProfile {
	public sealed class BotProfileAccessor(
		ITelegramBotClient telegramBotClient
	) {
		private User? _me;

		public async Task<User> GetBotProfileAsync(CancellationToken cancellationToken) {
			_me ??= await telegramBotClient.GetMe(cancellationToken);
			return _me;
		}
	}
}
