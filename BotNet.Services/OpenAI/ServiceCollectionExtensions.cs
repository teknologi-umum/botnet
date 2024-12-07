using BotNet.Services.OpenAI.Skills;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.OpenAI {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddOpenAiClient(this IServiceCollection services) {
			services.AddTransient<OpenAiClient>();
			services.AddTransient<OpenAiStreamingClient>();
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
