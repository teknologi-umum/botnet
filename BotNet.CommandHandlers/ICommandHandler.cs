using BotNet.Commands;
using MediatR;

namespace BotNet.CommandHandlers {
	public interface ICommandHandler<TCommand> : IRequestHandler<TCommand> where TCommand : ICommand {
		new Task Handle(TCommand command, CancellationToken cancellationToken);
	}
}
