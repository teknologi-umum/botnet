using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.OMDb {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddOmdbClient(this IServiceCollection services) {
			services.AddTransient<OmdbClient>();
			services.AddHttpClient<OmdbClient>();
			return services;
		}
	}
}
