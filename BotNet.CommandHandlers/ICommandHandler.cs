using Mediator;

namespace BotNet.CommandHandlers {
	public interface ICommandHandler<TCommand> : IRequestHandler<TCommand> where TCommand : Commands.ICommand {
		new ValueTask<Unit> Handle(TCommand command, CancellationToken cancellationToken);
	}
}
