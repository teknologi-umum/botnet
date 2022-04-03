namespace BotNet.Services.Piston.Models {
	public record ExecuteResult(
		string Language,
		string Version,
		ConsoleOutput Compile,
		ConsoleOutput Run
	);
}
