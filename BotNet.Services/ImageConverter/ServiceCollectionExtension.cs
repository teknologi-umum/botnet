using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.ImageConverter {
	public static class ServiceCollectionExtension {
		public static IServiceCollection AddImageConverter(this IServiceCollection services) {
			services.AddTransient<IcoToPngConverter>();
			return services;
		}
	}
}
