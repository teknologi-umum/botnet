using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Soundtrack {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddSoundtrackProvider(this IServiceCollection services) {
			services.AddTransient<SoundtrackProvider>();
			return services;
		}
	}
}
