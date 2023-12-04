using BotNet.Services.Stability.Skills;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Stability {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddStabilityClient(this IServiceCollection services) {
			services.AddSingleton<StabilityClient>();
			services.AddSingleton<ImageGenerationBot>();
			return services;
		}
	}
}
