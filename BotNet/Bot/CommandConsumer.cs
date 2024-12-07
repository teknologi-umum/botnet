﻿using System;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Commands;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BotNet.Bot {
	internal sealed class CommandConsumer(
		ICommandQueue commandQueue,
		IMediator mediator,
		ILogger<CommandConsumer> logger
	) : IHostedService {
		private CancellationTokenSource? _cancellationTokenSource;
		private TaskCompletionSource? _shutdownCompletionSource;

		public Task StartAsync(CancellationToken cancellationToken) {
			_cancellationTokenSource = new();
			_shutdownCompletionSource = new();

			Task.Run(async () => {
			Restart:
				try {
					while (_cancellationTokenSource is { IsCancellationRequested: false }) {
						// Execution strategy is defined here.
						// Current strategy is sequential, not concurrent, no DLQ, in single queue.
						ICommand command = await commandQueue.ReceiveAsync(_cancellationTokenSource.Token);
						await mediator.Send(command, _cancellationTokenSource.Token);
					}
				} catch (OperationCanceledException) {
					// Graceful shutdown
					_shutdownCompletionSource?.TrySetResult();
				} catch (Exception exc) {
					if (_cancellationTokenSource is not { IsCancellationRequested: false }) {
						logger.LogError(exc, "Command consumer crashed.");
						_shutdownCompletionSource?.TrySetException(exc);
						return;
					}

					logger.LogError(exc, "Command consumer crashed. Restarting in 5 seconds...");
					try {
						await Task.Delay(5000, _cancellationTokenSource.Token);
						goto Restart;
					} catch (OperationCanceledException) {
						// Graceful shutdown
						_shutdownCompletionSource?.TrySetResult();
					}
				}
			});

			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken) {
			if (_cancellationTokenSource != null) {
				await _cancellationTokenSource.CancelAsync();
			}

			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;
			if (_shutdownCompletionSource != null) {
				try {
					await _shutdownCompletionSource.Task;
				} catch (OperationCanceledException) {
					// Canceled means we're already shutting down
				}
			}
		}
	}
}
