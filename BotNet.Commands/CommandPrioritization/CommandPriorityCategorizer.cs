﻿using System.Collections.Immutable;
using Microsoft.Extensions.Options;

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

		public bool IsHomeGroup(long ChatId) {
			return _homeGroupChatIds.Contains(ChatId);
		}

		public bool IsVIPUser(long UserId) {
			return _vipUserIds.Contains(UserId);
		}
	}
}
