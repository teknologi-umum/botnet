using DynamicExpresso;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.DynamicExpresso {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddCSharpEvaluator(this IServiceCollection services) {
			services.AddSingleton<Interpreter>();
			services.AddTransient<CSharpEvaluator>();
			return services;
		}
	}
}
