using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.ClearScript {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddV8Evaluator(this IServiceCollection services) {
			services.AddTransient<V8Evaluator>();
			return services;
		}
	}
}
