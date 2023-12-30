namespace BotNet.Commands.CommandPrioritization {
	public sealed class CommandPrioritizationOptions {
		public required long[] HomeGroupChatIds { get; set; }
		public required long[] VIPUserIds { get; set; }
	}
}
