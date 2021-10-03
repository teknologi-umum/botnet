using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Brainfuck {
	public static class ServiceCollectionExtension {
		public static IServiceCollection AddBrainfuckTranspiler(this IServiceCollection services) {
			services.AddSingleton<BrainfuckTranspiler>();
			return services;
		}
	}
}
