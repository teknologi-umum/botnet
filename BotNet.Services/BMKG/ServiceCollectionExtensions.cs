using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.BMKG {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddBmkg(this IServiceCollection services) {
			services.AddTransient<LatestEarthQuake>();

			return services;
		}
	}
}
