namespace BotNet.Commands.CommandPrioritization {
	public sealed class CommandPrioritizationOptions {
		public string[] HomeGroupChatIds { get; init; } = [];
		public string[] VipUserIds { get; init; } = [];
	}
}
