using BotNet.Services.GoogleSheets.Options;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BotNet.Services.GoogleSheets {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddGoogleSheets(this IServiceCollection services) {
			services.AddSingleton(serviceProvider => {
				GoogleSheetsOptions options = serviceProvider.GetRequiredService<IOptions<GoogleSheetsOptions>>().Value;
				return new SheetsService(new BaseClientService.Initializer {
					ApiKey = options.ApiKey
				});
			});
			services.AddTransient<GoogleSheetsClient>();
			return services;
		}
	}
}
