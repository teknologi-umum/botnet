using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.OpenGraph {
	public static class ServiceCollectionExtension {
		public static IServiceCollection AddOpenGraph(this IServiceCollection services) {
			services.AddTransient<OpenGraphService>();
			return services;
		}
	}
}
