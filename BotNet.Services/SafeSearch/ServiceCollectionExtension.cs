using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.SafeSearch {
	public static class ServiceCollectionExtension {
		[ExcludeFromCodeCoverage]
		public static IServiceCollection AddSafeSearch(this IServiceCollection services) {
			services.AddSingleton<SafeSearchDictionary>();
			return services;
		}
	}
}
