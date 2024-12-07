using BotNet.Services.SQL;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.KokizzuVPSBenchmark {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddKokizzuVpsBenchmarkDataSource(this IServiceCollection services) {
			services.AddKeyedTransient<IScopedDataSource, VpsBenchmarkDataSource>("vps");
			return services;
		}
	}
}
