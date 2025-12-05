namespace BotNet.Services.Downdetector {
	public sealed record DowndetectorServiceStatus {
		public string ServiceName { get; init; } = default!;
		public bool? HasIssues { get; init; }
		public string? Description { get; init; }
	}
}
