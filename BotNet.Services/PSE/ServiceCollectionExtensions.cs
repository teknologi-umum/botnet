using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.PSE {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddPSEClient(this IServiceCollection services) {
			services.AddSingleton<PSECrawler>();
			services.AddHttpClient<PSEClient>();
			return services;
		}
	}
}
