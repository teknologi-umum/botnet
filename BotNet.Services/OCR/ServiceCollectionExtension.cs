using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.OCR {
	public static class ServiceCollectionExtension {
		public static IServiceCollection AddOCR(this IServiceCollection services) {
			services.AddSingleton<Reader>();
			return services;
		}
	}
}
