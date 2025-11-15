using System.Threading.Channels;
using BotNet.Commands;

namespace BotNet.CommandHandlers {
	internal sealed class CommandQueue : ICommandQueue {
		private readonly Channel<ICommand> _channel;
		private int _queueDepth;

		public CommandQueue() {
			// Use bounded channel to prevent unbounded memory growth during traffic spikes
			// Drop oldest commands when queue is full to maintain system stability
			BoundedChannelOptions options = new(capacity: 1000) {
				FullMode = BoundedChannelFullMode.DropOldest
			};
			_channel = Channel.CreateBounded<ICommand>(options);
			_queueDepth = 0;
		}

		public async Task DispatchAsync(ICommand command) {
			int currentDepth = Interlocked.Increment(ref _queueDepth);
			CommandQueueMetrics.SetQueueDepth(currentDepth);
			CommandQueueMetrics.RecordEnqueued();
			
			bool written = _channel.Writer.TryWrite(command);
			if (!written) {
				CommandQueueMetrics.RecordDropped();
				await _channel.Writer.WriteAsync(command);
			}
		}

		public async Task<ICommand> ReceiveAsync(CancellationToken cancellationToken) {
			ICommand command = await _channel.Reader.ReadAsync(cancellationToken);
			int currentDepth = Interlocked.Decrement(ref _queueDepth);
			CommandQueueMetrics.SetQueueDepth(currentDepth);
			return command;
		}
	}
}
