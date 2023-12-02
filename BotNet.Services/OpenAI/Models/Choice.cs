namespace BotNet.Services.OpenAI.Models {
	public record Choice(
		string? Text,
		int? Index,
		ChatMessage? Message,
		ChatMessage? Delta,
		Logprobs? Logprobs,
		string? FinishReason
	);
}
