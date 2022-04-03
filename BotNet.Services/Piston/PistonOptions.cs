namespace BotNet.Services.Piston {
	public class PistonOptions {
		public string RuntimesUrl { get; set; } = "https://emkc.org/api/v2/piston/runtimes";
		public string ExecuteUrl { get; set; } = "https://emkc.org/api/v2/piston/execute";
		public int MaxConcurrentExecutions { get; set; } = 2;
		public int CompileTimeout { get; set; } = 5000;
		public int RunTimeout { get; set; } = 3000;
		public int CompileMemoryLimit { get; set; } = -1;
		public int RunMemoryLimit { get; set; } = 200_000_000;
	}
}
