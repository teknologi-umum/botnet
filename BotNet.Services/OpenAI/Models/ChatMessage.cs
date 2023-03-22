namespace BotNet.Services.OpenAI.Models {
	public record ChatMessage(
		string Role,
		string Content
	);
}
