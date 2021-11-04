using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Piston {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddPistonClient(this IServiceCollection services) {
			services.AddTransient<PistonClient>();
			return services;
		}
	}
}
