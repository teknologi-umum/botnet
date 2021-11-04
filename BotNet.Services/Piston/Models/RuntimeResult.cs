using System.Collections.Immutable;

namespace BotNet.Services.Piston.Models {
	public record RuntimeResult(
		string Language,
		string Version,
		ImmutableHashSet<string> Aliases
	);
}
