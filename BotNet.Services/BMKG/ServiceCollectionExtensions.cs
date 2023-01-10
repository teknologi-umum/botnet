using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.BMKG {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddBMKG(this IServiceCollection services) {
			services.AddTransient<LatestEarthQuake>();

			return services;
		}
	}
}
