using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace BotNet.Bot {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddTelegramBot(this IServiceCollection services, string botToken) {
			services.AddHttpClient("tgwebhook")
				.AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(botToken, httpClient));
			services.AddSingleton<UpdateHandler>();
			services.AddSingleton<InlineQueryHandler>();
			return services;
		}
	}
}
