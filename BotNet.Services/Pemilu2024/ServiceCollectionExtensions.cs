using BotNet.Services.SQL;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Pemilu2024 {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddPemilu2024(this IServiceCollection services) {
			services.AddTransient<SirekapClient>();
			services.AddKeyedTransient<IScopedDataSource, PilpresDataSource>("pilpres");
			services.AddKeyedTransient<IScopedDataSource, PilegDPRPerProvinsiDataSource>("pileg_dpr_provinsi");
			services.AddKeyedTransient<IScopedDataSource, PilegDPRPerDapilDataSource>("pileg_dpr_dapil");
			return services;
		}
	}
}
