using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Sqlite {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddSqliteDatabases(this IServiceCollection services) {
			services.AddScoped<ScopedDatabase>();
			return services;
		}
	}
}
