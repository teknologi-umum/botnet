using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.ThisXDoesNotExist {
	public static class ServiceCollectionExtension {
		public static IServiceCollection AddThisXDoesNotExist(this IServiceCollection services) {
			services.AddTransient<ThisCatDoesNotExist>();
			services.AddTransient<ThisIdeaDoesNotExist>();
			services.AddTransient<ThisArtworkDoesNotExist>();
			return services;
		}
	}
}
