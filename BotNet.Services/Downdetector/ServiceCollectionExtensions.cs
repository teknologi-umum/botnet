using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Downdetector {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddDowndetectorClient(this IServiceCollection services) {
			services.AddHttpClient<DowndetectorClient>();
			return services;
		}
	}
}
