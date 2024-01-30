using System.Diagnostics.CodeAnalysis;

namespace BotNet.Commands.SenderAggregate {
	public abstract record SenderBase(
		SenderId Id,
		string Name
	) {
		public abstract string ChatGPTRole { get; }
	}

	public record HumanSender(
		SenderId Id,
		string Name
	) : SenderBase(Id, Name) {
		public override string ChatGPTRole => "user";

		public static bool TryCreate(
			Telegram.Bot.Types.User user,
			[NotNullWhen(true)] out HumanSender? humanSender
		) {
			if (user is {
				IsBot: false,
				Id: long senderId,
				FirstName: string senderFirstName,
				LastName: var senderLastName
			}) {
				humanSender = new HumanSender(
					Id: senderId,
					Name: senderLastName is { } ? $"{senderFirstName} {senderLastName}" : senderFirstName
				);
				return true;
			}

			humanSender = null;
			return false;
		}
	}

	public sealed record BotSender(
		SenderId Id,
		string Name
	) : SenderBase(Id, Name) {
		public override string ChatGPTRole => "assistant";

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
