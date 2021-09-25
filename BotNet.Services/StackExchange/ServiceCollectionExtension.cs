using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.StackExchange {
	public static class ServiceCollectionExtension {
		public static IServiceCollection AddStackExchangeClient(this IServiceCollection services) {
			services.AddTransient<StackExchangeClient>();
			return services;
		}
	}
}
