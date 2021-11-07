namespace BotNet.Services.OpenAI.Models {
	public record Choice(
		string? Text,
		int? Index,
		Logprobs? Logprobs,
		string? FinishReason
	);
}
