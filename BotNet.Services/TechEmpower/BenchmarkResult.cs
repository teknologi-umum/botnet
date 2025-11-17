namespace BotNet.Services.TechEmpower {
	public sealed record BenchmarkResult {
		public string Language { get; init; } = null!;
		public string Framework { get; init; } = null!;
		public double Score { get; init; }
		public int Rank { get; init; }
	}
}
