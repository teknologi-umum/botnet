using BotNet.Services.SQL;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.KokizzuVPSBenchmark {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddKokizzuVPSBenchmarkDataSource(this IServiceCollection services) {
			services.AddKeyedTransient<IScopedDataSource, VPSBenchmarkDataSource>("vps");
			return services;
		}
	}
}
