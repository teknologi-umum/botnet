using System.Collections.Immutable;

namespace BotNet.Services.Piston.Models {
	public record ExecutePayload(
		string Language,
		string Version,
		ImmutableList<FilePayload> Files,
		string? Stdin = null,
		ImmutableList<string>? Args = null,
		int CompileTimeout = 10000,
		int RunTimeout = 3000,
		int CompileMemoryLimit = -1,
		int RunMemoryLimit = -1
	);
}
