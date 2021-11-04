namespace BotNet.Services.Piston {
	public class PistonOptions {
		public string? BaseUrl { get; set; }
		public int MaxConcurrentExecutions { get; set; } = 2;
		public int CompileTimeout { get; set; } = 5000;
		public int RunTimeout { get; set; } = 3000;
		public int CompileMemoryLimit { get; set; } = -1;
		public int RunMemoryLimit { get; set; } = 200_000_000;
	}
}
