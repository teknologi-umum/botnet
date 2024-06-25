using BotNet.Services.SQL;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Antutu {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddAntutuDataSources(this IServiceCollection services) {
			services.AddTransient<AntutuScraper>();
			services.AddKeyedTransient<IScopedDataSource, AntutuAndroidDataSource>("antutu_android");
			return services;
		}
	}
}
