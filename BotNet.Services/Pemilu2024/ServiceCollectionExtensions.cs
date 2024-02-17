using BotNet.Services.SQL;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Pemilu2024 {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddPemilu2024(this IServiceCollection services) {
			services.AddTransient<SirekapClient>();
			services.AddTransient<PilegDPRDapilDataSource>();
			services.AddKeyedTransient<IScopedDataSource, PilpresDataSource>("pilpres");
			services.AddKeyedTransient<IScopedDataSource, PilegDPRPerProvinsiDataSource>("pileg_dpr_provinsi");
			services.AddKeyedTransient<IScopedDataSource, PilegDPRPerDapilDataSource>("pileg_dpr_dapil");
			services.AddPilegDPRDapilDataSource("1101");
			services.AddPilegDPRDapilDataSource("1102");
			services.AddPilegDPRDapilDataSource("5101");
			services.AddPilegDPRDapilDataSource("3601");
			services.AddPilegDPRDapilDataSource("3602");
			services.AddPilegDPRDapilDataSource("3603");
			services.AddPilegDPRDapilDataSource("1701");
			services.AddPilegDPRDapilDataSource("3401");
			services.AddPilegDPRDapilDataSource("3101");
			services.AddPilegDPRDapilDataSource("3102");
			services.AddPilegDPRDapilDataSource("3103");
			services.AddPilegDPRDapilDataSource("7501");
			services.AddPilegDPRDapilDataSource("1501");
			services.AddPilegDPRDapilDataSource("3201");
			services.AddPilegDPRDapilDataSource("3202");
			services.AddPilegDPRDapilDataSource("3203");
			services.AddPilegDPRDapilDataSource("3204");
			services.AddPilegDPRDapilDataSource("3209");
			services.AddPilegDPRDapilDataSource("3205");
			services.AddPilegDPRDapilDataSource("3206");
			services.AddPilegDPRDapilDataSource("3207");
			services.AddPilegDPRDapilDataSource("3208");
			services.AddPilegDPRDapilDataSource("3210");
			services.AddPilegDPRDapilDataSource("3211");
			services.AddPilegDPRDapilDataSource("3301");
			services.AddPilegDPRDapilDataSource("3302");
			services.AddPilegDPRDapilDataSource("3303");
			services.AddPilegDPRDapilDataSource("3304");
			services.AddPilegDPRDapilDataSource("3309");
			services.AddPilegDPRDapilDataSource("3305");
			services.AddPilegDPRDapilDataSource("3306");
			services.AddPilegDPRDapilDataSource("3307");
			services.AddPilegDPRDapilDataSource("3308");
			services.AddPilegDPRDapilDataSource("3310");
			services.AddPilegDPRDapilDataSource("3501");
			services.AddPilegDPRDapilDataSource("3502");
			services.AddPilegDPRDapilDataSource("3503");
			services.AddPilegDPRDapilDataSource("3504");
			services.AddPilegDPRDapilDataSource("3509");
			services.AddPilegDPRDapilDataSource("3505");
			services.AddPilegDPRDapilDataSource("3506");
			services.AddPilegDPRDapilDataSource("3507");
			services.AddPilegDPRDapilDataSource("3508");
			services.AddPilegDPRDapilDataSource("3510");
			services.AddPilegDPRDapilDataSource("3511");
			services.AddPilegDPRDapilDataSource("6101");
			services.AddPilegDPRDapilDataSource("6102");
			services.AddPilegDPRDapilDataSource("6301");
			services.AddPilegDPRDapilDataSource("6302");
			services.AddPilegDPRDapilDataSource("6201");
			services.AddPilegDPRDapilDataSource("6401");
			services.AddPilegDPRDapilDataSource("6501");
			services.AddPilegDPRDapilDataSource("1901");
			services.AddPilegDPRDapilDataSource("2101");
			services.AddPilegDPRDapilDataSource("1801");
			services.AddPilegDPRDapilDataSource("1802");
			services.AddPilegDPRDapilDataSource("8101");
			services.AddPilegDPRDapilDataSource("8201");
			services.AddPilegDPRDapilDataSource("5201");
			services.AddPilegDPRDapilDataSource("5202");
			services.AddPilegDPRDapilDataSource("5301");
			services.AddPilegDPRDapilDataSource("5302");
			services.AddPilegDPRDapilDataSource("9101");
			services.AddPilegDPRDapilDataSource("9201");
			services.AddPilegDPRDapilDataSource("9601");
			services.AddPilegDPRDapilDataSource("9501");
			services.AddPilegDPRDapilDataSource("9301");
			services.AddPilegDPRDapilDataSource("9401");
			services.AddPilegDPRDapilDataSource("1401");
			services.AddPilegDPRDapilDataSource("1402");
			services.AddPilegDPRDapilDataSource("7601");
			services.AddPilegDPRDapilDataSource("7301");
			services.AddPilegDPRDapilDataSource("7302");
			services.AddPilegDPRDapilDataSource("7303");
			services.AddPilegDPRDapilDataSource("7201");
			services.AddPilegDPRDapilDataSource("7401");
			services.AddPilegDPRDapilDataSource("7101");
			services.AddPilegDPRDapilDataSource("1301");
			services.AddPilegDPRDapilDataSource("1302");
			services.AddPilegDPRDapilDataSource("1601");
			services.AddPilegDPRDapilDataSource("1602");
			services.AddPilegDPRDapilDataSource("1201");
			services.AddPilegDPRDapilDataSource("1202");
			services.AddPilegDPRDapilDataSource("1203");
			return services;
		}

		private static IServiceCollection AddPilegDPRDapilDataSource(this IServiceCollection services, string kodeDapil) {
			services.AddKeyedTransient<IScopedDataSource, PilegDPRDapilDataSource>($"pileg_dpr_{kodeDapil}", (serviceProvider, key) => {
				PilegDPRDapilDataSource service = serviceProvider.GetRequiredService<PilegDPRDapilDataSource>();
				service.KodeDapil = kodeDapil;
				return service;
			});
			return services;
		}
	}
}
