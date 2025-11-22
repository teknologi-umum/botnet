using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Mermaid {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddMermaidRenderer(this IServiceCollection services) {
			services.AddHttpClient<MermaidRenderer>();
			return services;
		}
	}
}
