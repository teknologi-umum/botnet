using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Commands.CommandPrioritization {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddCommandPriorityCategorizer(this IServiceCollection services) {
			services.AddSingleton<CommandPriorityCategorizer>();
			return services;
		}
	}
}
