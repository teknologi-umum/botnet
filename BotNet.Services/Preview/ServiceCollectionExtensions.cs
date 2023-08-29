using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Preview {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddPreviewServices(this IServiceCollection services) {
			services.AddTransient<YoutubePreview>();
			return services;
		}
	}
}
