using BotNet.Commands.ChatAggregate;
using BotNet.Commands.CommandPrioritization;
using BotNet.Commands.Privilege;
using BotNet.Commands.SenderAggregate;
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
				RATE_LIMITER.ValidateActionRate(command.Chat.Id, command.Sender.Id);
			} catch (RateLimitExceededException) {
				// Silently reject commands after rate limit exceeded
				return Task.CompletedTask;
			}

			// Fire and forget
			Task.Run(async () => {
				try {
					switch (command) {
						case { Chat: PrivateChat, Sender: VIPSender }:
							await _telegramBotClient.SendTextMessageAsync(
								chatId: command.Chat.Id,
								text: $$"""
									👑 Anda adalah user VIP (ID: {{command.Sender.Id}})
									👑 GPT-4 tersedia
									👑 GPT-4 Vision tersedia
									👑 DALL-E 3 tersedia
									""",
								replyToMessageId: command.CommandMessageId,
								parseMode: ParseMode.Markdown,
								cancellationToken: cancellationToken
							);
							break;
						case { Chat: PrivateChat }:
							await _telegramBotClient.SendTextMessageAsync(
								chatId: command.Chat.Id,
								text: $$"""
									❌ Feature bot dibatasi di dalam private chat (ID: {{command.Sender.Id}})
									✅ GPT-3.5 tersedia
									❌ Vision tidak tersedia
									❌ Image generation tidak tersedia
									""",
								replyToMessageId: command.CommandMessageId,
								parseMode: ParseMode.Markdown,
								cancellationToken: cancellationToken
							);
							break;
						case { Chat: HomeGroupChat, Sender: VIPSender }:
							await _telegramBotClient.SendTextMessageAsync(
								chatId: command.Chat.Id,
								text: $$"""
										👑 Group {{command.Chat.Title}} (ID: {{command.Chat.Id}}) adalah home group
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
							break;
						case { Chat: HomeGroupChat }:
							await _telegramBotClient.SendTextMessageAsync(
								chatId: command.Chat.Id,
								text: $$"""
									👑 Group {{command.Chat.Title}} (ID: {{command.Chat.Id}}) adalah home group
									👑 GPT-4 tersedia
									👑 GPT-4 Vision tersedia
									✅ SDXL tersedia
									""",
								replyToMessageId: command.CommandMessageId,
								parseMode: ParseMode.Markdown,
								cancellationToken: cancellationToken
							);
							break;
						case { Chat: GroupChat, Sender: VIPSender }:
							await _telegramBotClient.SendTextMessageAsync(
								chatId: command.Chat.Id,
								text: $$"""
										⚠️ Bot dipakai di group selain home group (ID: {{command.Chat.Id}})
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
							break;
						case { Chat: GroupChat }:
							await _telegramBotClient.SendTextMessageAsync(
								chatId: command.Chat.Id,
								text: $$"""
									⚠️ Bot dipakai di group selain home group (ID: {{command.Chat.Id}})
									✅ GPT-3.5 tersedia
									❌ Vision tidak tersedia
									❌ Image generation tidak tersedia
									""",
								replyToMessageId: command.CommandMessageId,
								parseMode: ParseMode.Markdown,
								cancellationToken: cancellationToken
							);
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
