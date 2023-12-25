using BotNet.CommandHandlers.Eval;
using BotNet.CommandHandlers.FlipFlop;
using BotNet.Commands;
using BotNet.Commands.Eval;
using BotNet.Commands.FlipFlop;
using Microsoft.Extensions.DependencyInjection;
using BotNet.Commands.BotUpdate.Message;
using BotNet.CommandHandlers.BotUpdate.Message;

namespace BotNet.CommandHandlers {
	public static class _ {
		public static IServiceCollection AddCommandHandlers(this IServiceCollection services) {
			services.AddSingleton<ICommandQueue, CommandQueue>();
			services.AddTransient<ITelegramMessageCache, TelegramMessageCache>();
			services.AddTransient<ICommandHandler<SlashCommand>, SlashCommandHandler>();
			services.AddTransient<ICommandHandler<FlipFlopCommand>, FlipFlopCommandHandler>();
			services.AddTransient<ICommandHandler<EvalCommand>, EvalCommandHandler>();
			return services;
		}
	}
}
