using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.BotProfile {
	public static class ServiceCollectionExtension {
		public static IServiceCollection AddBotProfileAccessor(this IServiceCollection services) {
			services.AddSingleton<BotProfileAccessor>();
			return services;
		}
	}
}
