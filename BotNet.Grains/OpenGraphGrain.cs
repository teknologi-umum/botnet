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
		private Task<OpenGraphMetadata>? _metadataTask;
		private CancellationTokenSource? _cancellationTokenSource;

		public OpenGraphGrain(
			IServiceProvider serviceProvider
		) {
			_serviceProvider = serviceProvider;
		}

		public override Task OnActivateAsync() {
			_cancellationTokenSource = new();
			return Task.CompletedTask;
		}

		public override Task OnDeactivateAsync() {
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource?.Dispose();
			return Task.CompletedTask;
		}

		public async Task<OpenGraphMetadata?> GetMetadataAsync(TimeSpan timeout) {
			if (_metadataTask is null) {
				string url = this.GetPrimaryKeyString();
				_metadataTask = _serviceProvider
					.GetRequiredService<OpenGraphService>()
					.GetMetadataAsync(url, _cancellationTokenSource!.Token);
			} else if (_metadataTask.IsCompleted) {
				DelayDeactivation(TimeSpan.FromMinutes(1));
				return _metadataTask.Result;
			} else if (_metadataTask.IsCanceled) {
				DelayDeactivation(TimeSpan.FromMinutes(1));
				throw new OperationCanceledException();
			} else if (_metadataTask.IsFaulted) {
				DelayDeactivation(TimeSpan.FromMinutes(1));
				throw _metadataTask.Exception!;
			}

			Task completedTask = await Task.WhenAny(
				task1: _metadataTask,
				task2: Task.Delay(timeout));

			DelayDeactivation(TimeSpan.FromMinutes(1));

			return completedTask == _metadataTask ? _metadataTask.Result : null;
		}
	}
}
