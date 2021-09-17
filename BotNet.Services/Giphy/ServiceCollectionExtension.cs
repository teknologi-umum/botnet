using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Giphy {
	public static class ServiceCollectionExtension {
		public static IServiceCollection AddGiphyClient(this IServiceCollection services) {
			services.AddTransient<GiphyClient>();
			return services;
		}
	}
}
