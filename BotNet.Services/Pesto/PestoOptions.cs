namespace BotNet.Services.Pesto; 

public class PestoOptions {
	public string Token { get; set; }
	public string BaseUrl { get; set; } = "https://pesto.teknologiumum.com";
	public int MaxConcurrentExecutions { get; set; } = 3;
	public int CompileTimeout { get; set; } = 10_000;
	public int RunTimeout { get; set; } = 10_000;
	public int MemoryLimit { get; set; } = 200_000_000;
}
