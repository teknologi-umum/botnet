using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Tiktok {
	public static class ServiceCollectionExtension {
		public static IServiceCollection AddTiktokServices(this IServiceCollection services) {
			services.AddSingleton<TiktokLinkSanitizer>();
			return services;
		}
	}
}
