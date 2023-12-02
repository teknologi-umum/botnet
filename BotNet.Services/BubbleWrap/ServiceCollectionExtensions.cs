using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.BubbleWrap {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddBubbleWrapKeyboardGenerator(this IServiceCollection services) {
			services.AddTransient<BubbleWrapKeyboardGenerator>();
			return services;
		}
	}
}
