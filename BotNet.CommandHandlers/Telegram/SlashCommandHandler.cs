﻿using BotNet.Commands;
using BotNet.Commands.FlipFlop;
using BotNet.Commands.Telegram;

namespace BotNet.CommandHandlers.Telegram {
	public sealed class SlashCommandHandler : ICommandHandler<SlashCommand> {
		private readonly ICommandQueue _commandQueue;
		private readonly ITelegramMessageCache _telegramMessageCache;

		public SlashCommandHandler(
			ICommandQueue commandQueue,
			ITelegramMessageCache telegramMessageCache
		) {
			_commandQueue = commandQueue;
			_telegramMessageCache = telegramMessageCache;
		}

		public async Task Handle(SlashCommand command, CancellationToken cancellationToken) {
			switch (command.Command) {
				case "/flip":
				case "/flop":
				case "/flep":
				case "/flap":
					await _commandQueue.DispatchAsync(FlipFlopCommand.FromSlashCommand(
						slashCommand: command,
						repliedToMessage: command.ReplyToMessageId.HasValue
							? _telegramMessageCache.GetOrDefault(command.ReplyToMessageId.Value, command.ChatId)
							: null
					));
					break;
			}
		}
	}
}
