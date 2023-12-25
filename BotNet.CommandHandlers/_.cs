using BotNet.CommandHandlers.Eval;
using BotNet.CommandHandlers.FlipFlop;
using BotNet.CommandHandlers.Telegram;
using BotNet.Commands;
using BotNet.Commands.Eval;
using BotNet.Commands.FlipFlop;
using BotNet.Commands.Telegram;
using Microsoft.Extensions.DependencyInjection;

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
