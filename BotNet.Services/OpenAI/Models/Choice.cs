namespace BotNet.Services.OpenAI.Models {
	public record Choice(
		string? Text,
		int? Index,
		ChoiceChatMessage? Message,
		ChoiceChatMessage? Delta,
		Logprobs? Logprobs,
		string? FinishReason
	);

	public record ChoiceChatMessage(
		string Role,
		string Content
	);
}
