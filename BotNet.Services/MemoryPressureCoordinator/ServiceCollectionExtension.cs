using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.MemoryPressureCoordinator {
	public static class ServiceCollectionExtension {
		public static IServiceCollection AddMemoryPressureCoordinator(this IServiceCollection services) {
			services.AddSingleton<MemoryPressureSemaphore>();
			return services;
		}
	}
}
