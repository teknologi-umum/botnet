namespace BotNet.Services.Piston.Models {
	public record RunResult(
		string Stdout,
		string Stderr,
		string Output,
		int Code,
		string? Signal
	);
}
