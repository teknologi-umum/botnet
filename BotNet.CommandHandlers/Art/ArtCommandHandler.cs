using BotNet.Commands;
using BotNet.Commands.Art;
using BotNet.Services.OpenAI;

namespace BotNet.CommandHandlers.Art {
	public sealed class ArtCommandHandler(
		IntentDetector intentDetector,
		ICommandQueue commandQueue
	) : ICommandHandler<ArtCommand> {
		private readonly IntentDetector _intentDetector = intentDetector;
		private readonly ICommandQueue _commandQueue = commandQueue;

		public Task Handle(ArtCommand command, CancellationToken cancellationToken) {

		}
	}
}
