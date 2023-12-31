using BotNet.Commands.CommandPrioritization;
using BotNet.Commands.Privilege;
using BotNet.Services.RateLimit;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Privilege {
	public sealed class PrivilegeCommandHandler(
		ITelegramBotClient telegramBotClient,
		CommandPriorityCategorizer commandPriorityCategorizer
	) : ICommandHandler<PrivilegeCommand> {
		private static readonly RateLimiter RATE_LIMITER = RateLimiter.PerChat(1, TimeSpan.FromMinutes(1));

		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly CommandPriorityCategorizer _commandPriorityCategorizer = commandPriorityCategorizer;

		public Task Handle(PrivilegeCommand command, CancellationToken cancellationToken) {
			try {
				RATE_LIMITER.ValidateActionRate(command.ChatId, command.SenderId);
			} catch (RateLimitExceededException) {
				// Silently reject commands after rate limit exceeded
				return Task.CompletedTask;
			}

			// Fire and forget
			Task.Run(async () => {
				try {
					switch (command.ChatType) {
						case ChatType.Private:
							if (command.CommandPriority == CommandPriority.VIPChat) {
								await _telegramBotClient.SendTextMessageAsync(
									chatId: command.ChatId,
									text: $$"""
									👑 Anda adalah user VIP (ID: {{command.SenderId}})
									👑 GPT-4 tersedia
									👑 GPT-4 Vision tersedia
									👑 DALL-E 3 tersedia
									""",
									replyToMessageId: command.CommandMessageId,
									parseMode: ParseMode.Markdown,
									cancellationToken: cancellationToken
								);
							} else {
								await _telegramBotClient.SendTextMessageAsync(
									chatId: command.ChatId,
									text: $$"""
									❌ Feature bot dibatasi di dalam private chat (ID: {{command.SenderId}})
									✅ GPT-3.5 tersedia
									❌ Vision tidak tersedia
									❌ Image generation tidak tersedia
									""",
									replyToMessageId: command.CommandMessageId,
									parseMode: ParseMode.Markdown,
									cancellationToken: cancellationToken
								);
							}
							break;
						case ChatType.Group:
						case ChatType.Supergroup:
							if (command.CommandPriority == CommandPriority.VIPChat) {
								if (_commandPriorityCategorizer.IsHomeGroup(command.ChatId)) {
									await _telegramBotClient.SendTextMessageAsync(
										chatId: command.ChatId,
										text: $$"""
										👑 Group {{command.ChatTitle}} (ID: {{command.ChatId}}) adalah home group
										👑 GPT-4 tersedia
										👑 GPT-4 Vision tersedia
										✅ SDXL tersedia

										👑 Anda adalah user VIP
										👑 DALL-E 3 tersedia untuk Anda
										""",
										replyToMessageId: command.CommandMessageId,
										parseMode: ParseMode.Markdown,
										cancellationToken: cancellationToken
									);
								} else {
									await _telegramBotClient.SendTextMessageAsync(
										chatId: command.ChatId,
										text: $$"""
										⚠️ Bot dipakai di group selain home group (ID: {{command.ChatId}})
										✅ GPT-3.5 tersedia
										❌ Vision tidak tersedia
										❌ Image generation tidak tersedia

										👑 Anda adalah user VIP
										👑 GPT-4 tersedia untuk Anda
										👑 GPT-4 Vision tersedia untuk Anda
										👑 DALL-E 3 tersedia untuk Anda
										""",
										replyToMessageId: command.CommandMessageId,
										parseMode: ParseMode.Markdown,
										cancellationToken: cancellationToken
									);
								}
							} else if (command.CommandPriority == CommandPriority.HomeGroupChat) {
								await _telegramBotClient.SendTextMessageAsync(
									chatId: command.ChatId,
									text: $$"""
									👑 Group {{command.ChatTitle}} (ID: {{command.ChatId}}) adalah home group
									👑 GPT-4 tersedia
									👑 GPT-4 Vision tersedia
									✅ SDXL tersedia
									""",
									replyToMessageId: command.CommandMessageId,
									parseMode: ParseMode.Markdown,
									cancellationToken: cancellationToken
								);
							} else {
								await _telegramBotClient.SendTextMessageAsync(
									chatId: command.ChatId,
									text: $$"""
									⚠️ Bot dipakai di group selain home group (ID: {{command.ChatId}})
									✅ GPT-3.5 tersedia
									❌ Vision tidak tersedia
									❌ Image generation tidak tersedia
									""",
									replyToMessageId: command.CommandMessageId,
									parseMode: ParseMode.Markdown,
									cancellationToken: cancellationToken
								);
							}
							break;
					}
				} catch (OperationCanceledException) {
					// Terminate gracefully
				}
			});

			return Task.CompletedTask;
		}
	}
}
