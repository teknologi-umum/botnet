using BotNet.Commands;
using Mediator;
using BotNet.Commands.BotUpdate.CallbackQuery;
using BotNet.Commands.Pop;

namespace BotNet.CommandHandlers.BotUpdate.CallbackQuery {
	public sealed class CallbackQueryUpdateHandler(
		ICommandQueue commandQueue
	) : ICommandHandler<CallbackQueryUpdate> {
		public async ValueTask<Unit> Handle(CallbackQueryUpdate command, CancellationToken cancellationToken) {
			// Only handle callback queries with data
			if (command.CallbackQuery.Data is not { } data) {
				return default;
			}

			// Only handle callback queries with a colon in the data
			int colonIndex = data.IndexOf(':');
			if (colonIndex <= 0) {
				return default;
			}

			string commandName = data[..colonIndex];
			switch (commandName) {
				case "pop":
					if (BubbleWrapCallback.TryCreate(
						callbackQuery: command.CallbackQuery,
						out BubbleWrapCallback? bubbleWrapCallback
					)) {
						await commandQueue.DispatchAsync(bubbleWrapCallback);
					}
					break;
			}
	return default;
		}
	}
}
