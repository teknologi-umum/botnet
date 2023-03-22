namespace BotNet.Services.OpenAI.Models {
	public record Choice(
		string? Text,
		int? Index,
		ChatMessage? Message,
		Logprobs? Logprobs,
		string? FinishReason
	);
}
