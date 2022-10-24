using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Tokopedia {
	public static class ServiceCollectionExtension {
		public static IServiceCollection AddTokopediaServices(this IServiceCollection services) {
			services.AddSingleton<TokopediaLinkSanitizer>();

			return services;
		}
	}
}
