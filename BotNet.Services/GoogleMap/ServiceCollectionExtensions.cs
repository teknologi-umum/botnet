using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.GoogleMap {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddGoogleMaps(this IServiceCollection services) {
			services.AddTransient<GeoCode>();
			services.AddTransient<StaticMap>();
			services.AddTransient<PlacesClient>();

			return services;
		}
	}
}
