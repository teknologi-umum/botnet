using System.Threading.Channels;
using BotNet.Commands;

namespace BotNet.CommandHandlers {
	internal sealed class CommandQueue : ICommandQueue {
		private readonly Channel<ICommand> _channel;

		public CommandQueue() {
			_channel = Channel.CreateUnbounded<ICommand>();
		}

		public async Task DispatchAsync(ICommand command) {
			await _channel.Writer.WriteAsync(command);
		}

		public async Task<ICommand> ReceiveAsync(CancellationToken cancellationToken) {
			return await _channel.Reader.ReadAsync(cancellationToken);
		}
	}
}
