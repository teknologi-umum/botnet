using BotNet.Services.OpenAI.Skills;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.OpenAI {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddOpenAIClient(this IServiceCollection services) {
			services.AddTransient<OpenAIClient>();
			services.AddTransient<OpenAIStreamingClient>();
			services.AddTransient<ThreadTracker>();
			services.AddTransient<AssistantBot>();
			services.AddTransient<Translator>();
			services.AddTransient<FriendlyBot>();
			services.AddTransient<SarcasticBot>();
			services.AddTransient<AttachmentGenerator>();
			services.AddTransient<TldrGenerator>();
			services.AddTransient<IntentDetector>();
			services.AddTransient<VisionBot>();
			services.AddTransient<ImageGenerationBot>();
			return services;
		}
	}
}
