using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.SpamProtection {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddSpamProtection(this IServiceCollection services) {
			services.AddSingleton<SpamBanNotifier>();
			return services;
		}
	}
}
