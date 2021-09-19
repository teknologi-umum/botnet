using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.ColorCard {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddColorCardRenderer(this IServiceCollection services) {
			services.AddTransient<ColorCardRenderer>();
			return services;
		}
	}
}
