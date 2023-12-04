using System.Collections.Generic;

namespace BotNet.Services.Stability.Models {
	internal sealed record TextToImageResponse(
		List<Artifact> Artifacts
	);

	internal sealed record Artifact(
		string Base64,
		string FinishReason
	);
}
