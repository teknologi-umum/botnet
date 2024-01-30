using System.Diagnostics.CodeAnalysis;
using BotNet.Commands.CommandPrioritization;
using Telegram.Bot.Types.Enums;

namespace BotNet.Commands.ChatAggregate {
	public abstract record ChatBase {
		public ChatId Id { get; }
		public string? Title { get; }

		protected ChatBase(
			ChatId id,
			string? title
		) {
			Id = id;
			Title = title;
		}

		public static bool TryCreate(
			Telegram.Bot.Types.Chat telegramChat,
			CommandPriorityCategorizer priorityCategorizer,
			[NotNullWhen(true)] out ChatBase? chat
		) {
			chat = telegramChat switch {
				Telegram.Bot.Types.Chat { Type: ChatType.Private } => PrivateChat.FromTelegramChat(telegramChat),
				Telegram.Bot.Types.Chat { Type: ChatType.Group or ChatType.Supergroup } => priorityCategorizer.IsHomeGroup(telegramChat.Id)
					? HomeGroupChat.FromTelegramChat(telegramChat)
					: GroupChat.FromTelegramChat(telegramChat),
				_ => null
			};
			return chat is not null;
		}
	}

	public sealed record PrivateChat : ChatBase {
		private PrivateChat(
			ChatId id
		) : base(id, null) { }

		public static PrivateChat FromTelegramChat(
			Telegram.Bot.Types.Chat telegramChat
		) {
			if (telegramChat is not {
				Id: long chatId,
				Type: ChatType.Private
			}) {
				throw new ArgumentException("Telegram chat must be private.");
			}

			return new(
				id: new ChatId(chatId)
			);
		}
	}

	public record GroupChat : ChatBase {
		protected GroupChat(
			ChatId id,
			string title
		) : base(id, title) { }

		public static GroupChat FromTelegramChat(
			Telegram.Bot.Types.Chat telegramChat
		) {
			if (telegramChat is not {
				Id: long chatId,
				Title: string chatTitle,
				Type: ChatType.Group or ChatType.Supergroup
			}) {
				throw new ArgumentException("Telegram chat must be either a group or a supergroup.");
			}

			return new(
				id: new ChatId(chatId),
				title: chatTitle
			);
		}
	}

	public sealed record HomeGroupChat(
		ChatId Id,
		string Title
	) : GroupChat(Id, Title);
}
