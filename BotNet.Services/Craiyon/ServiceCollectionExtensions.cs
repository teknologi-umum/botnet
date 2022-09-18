using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Craiyon {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddCraiyonClient(this IServiceCollection services) {
			services.AddTransient<CraiyonClient>();
			return services;
		}
	}
}
