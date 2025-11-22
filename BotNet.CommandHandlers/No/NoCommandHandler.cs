using BotNet.Commands.ChatAggregate;
using Mediator;
using BotNet.Commands.No;
using BotNet.Commands.SenderAggregate;
using BotNet.Services.NoAsAService;
using BotNet.Services.RateLimit;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.No {
	public sealed class NoCommandHandler(
		ITelegramBotClient telegramBotClient,
		NoAsAServiceClient noAsAServiceClient
	) : ICommandHandler<NoCommand> {
		private static readonly RateLimiter HomeGroupChatRateLimiter = RateLimiter.PerUserPerChat(5, TimeSpan.FromMinutes(2));
		private static readonly RateLimiter GroupChatRateLimiter = RateLimiter.PerUserPerChat(2, TimeSpan.FromMinutes(2));
		private static readonly RateLimiter PrivateChatRateLimiter = RateLimiter.PerUserPerChat(20, TimeSpan.FromMinutes(2));

		public async ValueTask<Unit> Handle(NoCommand command, CancellationToken cancellationToken) {
			// No rate limit for VIP
			if (command.Command.Sender is not VipSender) {
				try {
					// Apply different rate limits based on chat type
					switch (command.Command.Chat) {
						case HomeGroupChat:
							HomeGroupChatRateLimiter.ValidateActionRate(
								chatId: command.Command.Chat.Id,
								userId: command.Command.Sender.Id
							);
							break;
						case GroupChat:
							GroupChatRateLimiter.ValidateActionRate(
								chatId: command.Command.Chat.Id,
								userId: command.Command.Sender.Id
							);
							break;
						case PrivateChat:
							PrivateChatRateLimiter.ValidateActionRate(
								chatId: command.Command.Chat.Id,
								userId: command.Command.Sender.Id
							);
							break;
		return default;
					}
			} catch (RateLimitExceededException exc) {
				await telegramBotClient.SendMessage(
					chatId: command.Command.Chat.Id,
					text: $"<code>Kebanyakan say no. Coba lagi {exc.Cooldown}.</code>",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters {
							MessageId = command.Command.MessageId
						},
						cancellationToken: cancellationToken
					);
					return default;
				}
			}

			string reason = await noAsAServiceClient.GetNoReasonAsync(cancellationToken);

			await telegramBotClient.SendMessage(
				chatId: command.Command.Chat.Id,
				text: $"❌ {reason}",
				parseMode: ParseMode.Html,
				replyParameters: new ReplyParameters {
					MessageId = command.Command.MessageId
				},
				cancellationToken: cancellationToken
			);
	return default;
		}
	}
}
