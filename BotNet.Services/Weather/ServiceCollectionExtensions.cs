using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Weather {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddWeatherService(this IServiceCollection services) {
			services.AddTransient<WttrInWeather>();

			return services;
		}
	}
}
