using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.ChineseCalendar {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddChineseCalendarScraper(this IServiceCollection services) {
			services.AddTransient<ChineseCalendarScraper>();
			return services;
		}
	}
}
