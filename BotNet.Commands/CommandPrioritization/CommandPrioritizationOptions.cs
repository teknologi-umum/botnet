namespace BotNet.Commands.CommandPrioritization {
	public sealed class CommandPrioritizationOptions {
		public string[] HomeGroupChatIds { get; set; } = [];
		public string[] VIPUserIds { get; set; } = [];
	}
}
