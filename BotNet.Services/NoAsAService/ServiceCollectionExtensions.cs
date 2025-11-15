using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.NoAsAService {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddNoAsAServiceClient(this IServiceCollection services) {
			services.AddTransient<NoAsAServiceClient>();
			return services;
		}
	}
}
