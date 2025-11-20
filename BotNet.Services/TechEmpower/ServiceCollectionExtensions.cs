using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.TechEmpower {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddTechEmpowerScraper(this IServiceCollection services) {
			services.AddHttpClient<TechEmpowerScraper>();
			services.AddTransient<BenchmarkChartRenderer>();
			return services;
		}
	}
}
