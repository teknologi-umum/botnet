using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.Giphy {
	public static class ServiceCollectionExtension {
		[Obsolete("GiphyClient is deprecated. Use TenorClient instead.")]
		[ExcludeFromCodeCoverage]
		public static IServiceCollection AddGiphyClient(this IServiceCollection services) {
			services.AddTransient<GiphyClient>();
			return services;
		}
	}
}
