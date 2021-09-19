using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Typography {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddFontService(this IServiceCollection services) {
			services.AddSingleton<BotNetFontService>();
			return services;
		}
	}
}
