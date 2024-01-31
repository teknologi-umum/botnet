using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Gemini {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddGeminiClient(this IServiceCollection services) {
			services.AddTransient<GeminiClient>();
			return services;
		}
	}
}
