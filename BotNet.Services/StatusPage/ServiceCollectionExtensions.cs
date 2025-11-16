using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.StatusPage {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddStatusPageClient(this IServiceCollection services) {
			services.AddTransient<StatusPageClient>();
			services.AddHttpClient<StatusPageClient>();
			return services;
		}
	}
}
