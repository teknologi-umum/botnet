using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Meme {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddMemeGenerator(this IServiceCollection services) {
			services.AddTransient<MemeGenerator>();
			return services;
		}
	}
}
