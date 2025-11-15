using BotNet.Commands.ChatAggregate;
using BotNet.Commands.Privilege;
using BotNet.Commands.SenderAggregate;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Privilege {
	public sealed class PrivilegeCommandHandler(
		ITelegramBotClient telegramBotClient,
		ILogger<PrivilegeCommandHandler> logger
	) : ICommandHandler<PrivilegeCommand> {
		private static readonly RateLimiter RateLimiter = RateLimiter.PerChat(1, TimeSpan.FromMinutes(1));

		public Task Handle(PrivilegeCommand command, CancellationToken cancellationToken) {
			try {
				RateLimiter.ValidateActionRate(command.Chat.Id, command.Sender.Id);
			} catch (RateLimitExceededException) {
				// Silently reject commands after rate limit exceeded
				return Task.CompletedTask;
			}

			// Fire and forget
			BackgroundTask.Run(async () => {
				try {
					switch (command) {
						case { Chat: PrivateChat, Sender: VipSender }:
							await telegramBotClient.SendMessage(
								chatId: command.Chat.Id,
								text: $$"""
									👑 Anda adalah user VIP (ID: {{command.Sender.Id}})
									👑 Gemini Pro tersedia
									👑 GPT-4 tersedia
									👑 GPT-4 Vision tersedia
									👑 DALL-E 3 tersedia
									""",
								replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
								parseMode: ParseMode.Markdown,
								cancellationToken: cancellationToken
							);
							break;
						case { Chat: PrivateChat }:
							await telegramBotClient.SendMessage(
								chatId: command.Chat.Id,
								text: $$"""
									❌ Feature bot dibatasi di dalam private chat (ID: {{command.Sender.Id}})
									✅ Gemini Pro tersedia
									✅ GPT-3.5 tersedia
									❌ Vision tidak tersedia
									❌ Image generation tidak tersedia
									""",
								replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
								parseMode: ParseMode.Markdown,
								cancellationToken: cancellationToken
							);
							break;
						case { Chat: HomeGroupChat, Sender: VipSender }:
							await telegramBotClient.SendMessage(
								chatId: command.Chat.Id,
								text: $$"""
										👑 Group {{command.Chat.Title}} (ID: {{command.Chat.Id}}) adalah home group
										👑 Gemini Pro tersedia
										👑 GPT-4 tersedia
										👑 GPT-4 Vision tersedia
										✅ SDXL tersedia

										👑 Anda adalah user VIP
										👑 DALL-E 3 tersedia untuk Anda
										""",
								replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
								parseMode: ParseMode.Markdown,
								cancellationToken: cancellationToken
							);
							break;
						case { Chat: HomeGroupChat }:
							await telegramBotClient.SendMessage(
								chatId: command.Chat.Id,
								text: $$"""
									👑 Group {{command.Chat.Title}} (ID: {{command.Chat.Id}}) adalah home group
									👑 Gemini Pro tersedia
									👑 GPT-4 tersedia
									👑 GPT-4 Vision tersedia
									✅ SDXL tersedia
									""",
								replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
								parseMode: ParseMode.Markdown,
								cancellationToken: cancellationToken
							);
							break;
						case { Chat: GroupChat, Sender: VipSender }:
							await telegramBotClient.SendMessage(
								chatId: command.Chat.Id,
								text: $$"""
										⚠️ Bot dipakai di group selain home group (ID: {{command.Chat.Id}})
										✅ Gemini Pro tersedia
										✅ GPT-3.5 tersedia
										❌ Vision tidak tersedia
										❌ Image generation tidak tersedia

										👑 Anda adalah user VIP
										👑 GPT-4 tersedia untuk Anda
										👑 GPT-4 Vision tersedia untuk Anda
										👑 DALL-E 3 tersedia untuk Anda
										""",
								replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
								parseMode: ParseMode.Markdown,
								cancellationToken: cancellationToken
							);
							break;
						case { Chat: GroupChat }:
							await telegramBotClient.SendMessage(
								chatId: command.Chat.Id,
								text: $$"""
									⚠️ Bot dipakai di group selain home group (ID: {{command.Chat.Id}})
									✅ Gemini Pro tersedia
									✅ GPT-3.5 tersedia
									❌ Vision tidak tersedia
									❌ Image generation tidak tersedia
									""",
								replyParameters: new ReplyParameters { MessageId = command.CommandMessageId },
								parseMode: ParseMode.Markdown,
								cancellationToken: cancellationToken
							);
							break;
					}
				} catch (OperationCanceledException) {
					// Terminate gracefully
				}
			}, logger);

			return Task.CompletedTask;
		}
	}
}
