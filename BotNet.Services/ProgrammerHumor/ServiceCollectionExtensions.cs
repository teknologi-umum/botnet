using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.ProgrammerHumor {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddProgrammerHumorScraper(this IServiceCollection services) {
			services.AddHttpClient<ProgrammerHumorScraper>();
			return services;
		}
	}
}
