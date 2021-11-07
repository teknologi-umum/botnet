using System.Collections.Generic;

namespace BotNet.Services.OpenAI.Models {
	public record Logprobs(
		List<string> Tokens,
		List<double> TokenLogprobs,
		List<Dictionary<string, double>> TopLogprobs,
		List<int> TextOffsets
	);
}
