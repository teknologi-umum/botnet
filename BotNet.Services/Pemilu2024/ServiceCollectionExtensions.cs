using BotNet.Services.SQL;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Pemilu2024 {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddPemilu2024(this IServiceCollection services) {
			services.AddTransient<SirekapClient>();
			services.AddKeyedTransient<IScopedDataSource, PilpresDataSource>("pilpres");
			return services;
		}
	}
}
