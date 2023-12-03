namespace BotNet.Services.OpenAI.Skills {
	public sealed class VisionBot(
		OpenAIStreamingClient openAIStreamingClient
	) {
		private readonly OpenAIStreamingClient _openAIStreamingClient = openAIStreamingClient;
	}
}
