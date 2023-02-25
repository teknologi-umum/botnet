using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Pesto; 

public static class ServiceCollectionExtensions {
	public static IServiceCollection AddPestoClient(this IServiceCollection serviceCollection) {
		serviceCollection.AddTransient<PestoClient>();
		return serviceCollection;
	}
}
