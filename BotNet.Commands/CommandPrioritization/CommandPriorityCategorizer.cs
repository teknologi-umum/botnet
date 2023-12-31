using System.Collections.Immutable;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.CommandPrioritization {
	public sealed class CommandPriorityCategorizer {
		private readonly ImmutableHashSet<long> _homeGroupChatIds;
		private readonly ImmutableHashSet<long> _vipUserIds;

		public CommandPriorityCategorizer(
			IOptions<CommandPrioritizationOptions> optionsAccessor
		) {
			_homeGroupChatIds = optionsAccessor.Value.HomeGroupChatIds
				.SelectMany(chatId => chatId.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
				.Select(chatIdStr => long.TryParse(chatIdStr, out long chatId) ? (long?)chatId : null)
				.Where(chatId => chatId.HasValue)
				.Select(chatId => chatId!.Value)
				.Distinct()
				.ToImmutableHashSet();

			_vipUserIds = optionsAccessor.Value.VIPUserIds
				.SelectMany(userId => userId.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
				.Select(userIdStr => long.TryParse(userIdStr, out long userId) ? (long?)userId : null)
				.Where(userId => userId.HasValue)
				.Select(userId => userId!.Value)
				.Distinct()
				.ToImmutableHashSet();
		}

		public CommandPriority Categorize(Message message) {
			if (message.From is null) {
				return CommandPriority.Void;
			}

			if (message.From.IsBot) {
				return CommandPriority.Void;
			}

			if (_vipUserIds.Contains(message.From.Id)) {
				return CommandPriority.VIPChat;
			}

			if (_homeGroupChatIds.Contains(message.Chat.Id)) {
				return CommandPriority.HomeGroupChat;
			}

			switch (message.Chat.Type) {
				case ChatType.Private:
					return CommandPriority.PrivateChat;
				case ChatType.Group:
				case ChatType.Channel:
				case ChatType.Supergroup:
					return CommandPriority.GroupChat;
				default:
					return CommandPriority.Void;
			}
		}

		public bool IsHomeGroup(long ChatId) {
			return _homeGroupChatIds.Contains(ChatId);
		}
	}
}
