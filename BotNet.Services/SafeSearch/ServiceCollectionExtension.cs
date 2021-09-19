using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.SafeSearch {
	public static class ServiceCollectionExtension {
		public static IServiceCollection AddSafeSearch(this IServiceCollection services) {
			services.AddSingleton<SafeSearchDictionary>();
			return services;
		}
	}
}
