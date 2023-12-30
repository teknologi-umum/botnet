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
			_homeGroupChatIds = optionsAccessor.Value.HomeGroupChatIds.ToImmutableHashSet();
			_vipUserIds = optionsAccessor.Value.VIPUserIds.ToImmutableHashSet();
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
	}
}
