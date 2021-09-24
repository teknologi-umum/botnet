using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Github {
	public static class ServiceCollectionExtension {
		public static IServiceCollection AddGithubClient(this IServiceCollection services) {
			services.AddTransient<GithubClient>();
			return services;
		}
	}
}
