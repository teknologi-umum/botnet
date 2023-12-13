using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Primbon {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddPrimbonScraper(this IServiceCollection services) {
			services.AddTransient<PrimbonScraper>();
			return services;
		}
	}
}
