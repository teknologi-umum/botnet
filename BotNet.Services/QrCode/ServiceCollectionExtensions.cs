using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.QrCode {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddQrCodeGenerator(this IServiceCollection services) {
			services.AddTransient<QrCodeGenerator>();
			return services;
		}
	}
}
