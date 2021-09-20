using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.DuckDuckGo {
	public static class ServiceCollectionExtension {
		[ExcludeFromCodeCoverage]
		public static IServiceCollection AddDuckDuckGo(this IServiceCollection services) {
			services.AddTransient<DuckDuckGoClient>();
			return services;
		}
	}
}
