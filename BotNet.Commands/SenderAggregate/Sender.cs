using System.Diagnostics.CodeAnalysis;
using BotNet.Commands.CommandPrioritization;

namespace BotNet.Commands.SenderAggregate {
	public abstract record SenderBase(
		SenderId Id,
		string Name
	) {
		public abstract string ChatGPTRole { get; }
		public abstract string GeminiRole { get; }
	}

	public record HumanSender(
		SenderId Id,
		string Name
	) : SenderBase(Id, Name) {
		public override string ChatGPTRole => "user";
		public override string GeminiRole => "user";

		public static bool TryCreate(
			Telegram.Bot.Types.User user,
			CommandPriorityCategorizer commandPriorityCategorizer,
			[NotNullWhen(true)] out HumanSender? humanSender
		) {
			if (user is not {
				IsBot: false,
				Id: long senderId,
				FirstName: string senderFirstName,
				LastName: var senderLastName
			}) {
				humanSender = null;
				return false;
			}

			if (commandPriorityCategorizer.IsVIPUser(senderId)) {
				humanSender = new VIPSender(
					Id: senderId,
					Name: senderLastName is { } ? $"{senderFirstName} {senderLastName}" : senderFirstName
				);
				return true;
			}

			humanSender = new HumanSender(
				Id: senderId,
				Name: senderLastName is { } ? $"{senderFirstName} {senderLastName}" : senderFirstName
			);
			return true;
		}
	}

	public sealed record BotSender(
		SenderId Id,
		string Name
	) : SenderBase(Id, Name) {
		public override string ChatGPTRole => "assistant";
		public override string GeminiRole => "model";

		public static bool TryCreate(
			Telegram.Bot.Types.User user,
			[NotNullWhen(true)] out BotSender? botSender
		) {
			if (user is {
				IsBot: true,
				Id: long senderId,
				FirstName: string senderFirstName,
				LastName: var senderLastName
			}) {
				botSender = new BotSender(
					Id: senderId,
					Name: senderLastName is { } ? $"{senderFirstName} {senderLastName}" : senderFirstName
				);
				return true;
			}

			botSender = null;
			return false;
		}
	}

	public sealed record VIPSender(
		SenderId Id,
		string Name
	) : HumanSender(Id, Name);
}
