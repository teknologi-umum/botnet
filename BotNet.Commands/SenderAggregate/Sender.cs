﻿using System.Diagnostics.CodeAnalysis;
using BotNet.Commands.CommandPrioritization;

namespace BotNet.Commands.SenderAggregate {
	public abstract record SenderBase(
		SenderId Id,
		string Name
	) {
		public abstract string ChatGptRole { get; }
		public abstract string GeminiRole { get; }
	}

	public record HumanSender(
		SenderId Id,
		string Name
	) : SenderBase(Id, Name) {
		public override string ChatGptRole => "user";
		public override string GeminiRole => "user";

		public static bool TryCreate(
			Telegram.Bot.Types.User user,
			CommandPriorityCategorizer commandPriorityCategorizer,
			[NotNullWhen(true)] out HumanSender? humanSender
		) {
			if (user is not {
				IsBot: false,
				Id: long senderId,
				FirstName: { } senderFirstName,
				LastName: var senderLastName
			}) {
				humanSender = null;
				return false;
			}

			if (commandPriorityCategorizer.IsVipUser(senderId)) {
				humanSender = new VipSender(
					Id: senderId,
					Name: senderLastName is not null
						? $"{senderFirstName} {senderLastName}" : senderFirstName
				);
				return true;
			}

			humanSender = new HumanSender(
				Id: senderId,
				Name: senderLastName is not null
					? $"{senderFirstName} {senderLastName}" : senderFirstName
			);
			return true;
		}
	}

	public sealed record BotSender(
		SenderId Id,
		string Name
	) : SenderBase(Id, Name) {
		public override string ChatGptRole => "assistant";
		public override string GeminiRole => "model";

		public static bool TryCreate(
			Telegram.Bot.Types.User user,
			[NotNullWhen(true)] out BotSender? botSender
		) {
			if (user is {
				IsBot: true,
				Id: long senderId,
				FirstName: { } senderFirstName,
				LastName: var senderLastName
			}) {
				botSender = new BotSender(
					Id: senderId,
					Name: senderLastName is not null
						? $"{senderFirstName} {senderLastName}" : senderFirstName
				);
				return true;
			}

			botSender = null;
			return false;
		}
	}

	public sealed record VipSender(
		SenderId Id,
		string Name
	) : HumanSender(Id, Name);
}
