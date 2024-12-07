using BotNet.Services.SQL;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Pemilu2024 {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddPemilu2024(this IServiceCollection services) {
			services.AddTransient<SirekapClient>();
			services.AddTransient<PilegDprDapilDataSource>();
			services.AddKeyedTransient<IScopedDataSource, PilpresDataSource>("pilpres");
			services.AddKeyedTransient<IScopedDataSource, PilegDprPerProvinsiDataSource>("pileg_dpr_provinsi");
			services.AddKeyedTransient<IScopedDataSource, PilegDprPerDapilDataSource>("pileg_dpr_dapil");
			services.AddPilegDprDapilDataSource("1101");
			services.AddPilegDprDapilDataSource("1102");
			services.AddPilegDprDapilDataSource("5101");
			services.AddPilegDprDapilDataSource("3601");
			services.AddPilegDprDapilDataSource("3602");
			services.AddPilegDprDapilDataSource("3603");
			services.AddPilegDprDapilDataSource("1701");
			services.AddPilegDprDapilDataSource("3401");
			services.AddPilegDprDapilDataSource("3101");
			services.AddPilegDprDapilDataSource("3102");
			services.AddPilegDprDapilDataSource("3103");
			services.AddPilegDprDapilDataSource("7501");
			services.AddPilegDprDapilDataSource("1501");
			services.AddPilegDprDapilDataSource("3201");
			services.AddPilegDprDapilDataSource("3202");
			services.AddPilegDprDapilDataSource("3203");
			services.AddPilegDprDapilDataSource("3204");
			services.AddPilegDprDapilDataSource("3209");
			services.AddPilegDprDapilDataSource("3205");
			services.AddPilegDprDapilDataSource("3206");
			services.AddPilegDprDapilDataSource("3207");
			services.AddPilegDprDapilDataSource("3208");
			services.AddPilegDprDapilDataSource("3210");
			services.AddPilegDprDapilDataSource("3211");
			services.AddPilegDprDapilDataSource("3301");
			services.AddPilegDprDapilDataSource("3302");
			services.AddPilegDprDapilDataSource("3303");
			services.AddPilegDprDapilDataSource("3304");
			services.AddPilegDprDapilDataSource("3309");
			services.AddPilegDprDapilDataSource("3305");
			services.AddPilegDprDapilDataSource("3306");
			services.AddPilegDprDapilDataSource("3307");
			services.AddPilegDprDapilDataSource("3308");
			services.AddPilegDprDapilDataSource("3310");
			services.AddPilegDprDapilDataSource("3501");
			services.AddPilegDprDapilDataSource("3502");
			services.AddPilegDprDapilDataSource("3503");
			services.AddPilegDprDapilDataSource("3504");
			services.AddPilegDprDapilDataSource("3509");
			services.AddPilegDprDapilDataSource("3505");
			services.AddPilegDprDapilDataSource("3506");
			services.AddPilegDprDapilDataSource("3507");
			services.AddPilegDprDapilDataSource("3508");
			services.AddPilegDprDapilDataSource("3510");
			services.AddPilegDprDapilDataSource("3511");
			services.AddPilegDprDapilDataSource("6101");
			services.AddPilegDprDapilDataSource("6102");
			services.AddPilegDprDapilDataSource("6301");
			services.AddPilegDprDapilDataSource("6302");
			services.AddPilegDprDapilDataSource("6201");
			services.AddPilegDprDapilDataSource("6401");
			services.AddPilegDprDapilDataSource("6501");
			services.AddPilegDprDapilDataSource("1901");
			services.AddPilegDprDapilDataSource("2101");
			services.AddPilegDprDapilDataSource("1801");
			services.AddPilegDprDapilDataSource("1802");
			services.AddPilegDprDapilDataSource("8101");
			services.AddPilegDprDapilDataSource("8201");
			services.AddPilegDprDapilDataSource("5201");
			services.AddPilegDprDapilDataSource("5202");
			services.AddPilegDprDapilDataSource("5301");
			services.AddPilegDprDapilDataSource("5302");
			services.AddPilegDprDapilDataSource("9101");
			services.AddPilegDprDapilDataSource("9201");
			services.AddPilegDprDapilDataSource("9601");
			services.AddPilegDprDapilDataSource("9501");
			services.AddPilegDprDapilDataSource("9301");
			services.AddPilegDprDapilDataSource("9401");
			services.AddPilegDprDapilDataSource("1401");
			services.AddPilegDprDapilDataSource("1402");
			services.AddPilegDprDapilDataSource("7601");
			services.AddPilegDprDapilDataSource("7301");
			services.AddPilegDprDapilDataSource("7302");
			services.AddPilegDprDapilDataSource("7303");
			services.AddPilegDprDapilDataSource("7201");
			services.AddPilegDprDapilDataSource("7401");
			services.AddPilegDprDapilDataSource("7101");
			services.AddPilegDprDapilDataSource("1301");
			services.AddPilegDprDapilDataSource("1302");
			services.AddPilegDprDapilDataSource("1601");
			services.AddPilegDprDapilDataSource("1602");
			services.AddPilegDprDapilDataSource("1201");
			services.AddPilegDprDapilDataSource("1202");
			services.AddPilegDprDapilDataSource("1203");
			return services;
		}

		private static IServiceCollection AddPilegDprDapilDataSource(this IServiceCollection services, string kodeDapil) {
			services.AddKeyedTransient<IScopedDataSource, PilegDprDapilDataSource>($"pileg_dpr_{kodeDapil}", (serviceProvider, _) => {
				PilegDprDapilDataSource service = serviceProvider.GetRequiredService<PilegDprDapilDataSource>();
				service.KodeDapil = kodeDapil;
				return service;
			});
			return services;
		}
	}
}
