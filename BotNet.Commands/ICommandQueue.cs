namespace BotNet.Commands {
	public interface ICommandQueue {
		Task DispatchAsync(ICommand command);
		Task<ICommand> ReceiveAsync(CancellationToken cancellationToken);
	}
}
