using System;
using System.Threading;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using BotNet.Services.OpenGraph;
using BotNet.Services.OpenGraph.Models;
using Microsoft.Extensions.DependencyInjection;
using Orleans;

namespace BotNet.Grains {
	public class OpenGraphGrain : Grain, IOpenGraphGrain {
		private readonly IServiceProvider _serviceProvider;
		private OpenGraphMetadata? _metadata;

		public OpenGraphGrain(
			IServiceProvider serviceProvider
		) {
			_serviceProvider = serviceProvider;
		}

		public async Task<OpenGraphMetadata> GetMetadataAsync() {
			if (_metadata is null) {
				string url = this.GetPrimaryKeyString();

				_metadata = await _serviceProvider
					.GetRequiredService<OpenGraphService>()
					.GetMetadataAsync(url, CancellationToken.None);
			}
			return _metadata;
		}
	}
}
