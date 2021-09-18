using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Tenor {
	public static class ServiceCollectionExtension {
		public static IServiceCollection AddTenorClient(this IServiceCollection services) {
			services.AddSingleton<TenorClient>();
			return services;
		}
	}
}
