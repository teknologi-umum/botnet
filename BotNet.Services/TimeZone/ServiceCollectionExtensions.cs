using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.TimeZone {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddTimeZoneService(this IServiceCollection services) {
			services.AddTransient<TimeZoneService>();
			return services;
		}
	}
}
