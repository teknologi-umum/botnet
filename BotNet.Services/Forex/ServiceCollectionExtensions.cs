using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Forex {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddForexRatesService(this IServiceCollection services) {
			services.AddTransient<ForexRates>();
			return services;
		}
	}
}
