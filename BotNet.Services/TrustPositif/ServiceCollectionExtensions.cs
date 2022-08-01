using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.TrustPositif {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddTrustPositifLookup(this IServiceCollection services) {
			services.AddTransient<TrustPositifClient>();
			services.AddTransient<TrustPositifLookup>();
			return services;
		}
	}
}
