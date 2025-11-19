using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Plot {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddMathPlotRenderer(this IServiceCollection services) {
			services.AddTransient<MathPlotRenderer>();
			return services;
		}
	}
}
