using System;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.OpenAI;
using Telegram.Bot.Types;

namespace BotNet.Controllers {
	public class ArtController : BotControllerBase {
		private readonly Services.Stability.Skills.ImageGenerationBot _imageGenerationBot;
		private readonly ThreadTracker _threadTracker;

		public ArtController(
			Services.Stability.Skills.ImageGenerationBot imageGenerationBot,
			ThreadTracker threadTracker
		) {
			_imageGenerationBot = imageGenerationBot;
			_threadTracker = threadTracker;
		}

		public async Task GetRandomArtAsync(
			string? commandArgument,
			CancellationToken cancellationToken
		) {
			Message busyMessage = await ReplyMarkdownAsync("Generating image… ⏳", cancellationToken);

			byte[] generatedImage = await _imageGenerationBot.GenerateImageAsync(commandArgument, cancellationToken);

			await TryDeleteMessageAsync(busyMessage, cancellationToken);

			Message replyMessage = await ReplyPhotoAsync(generatedImage, cancellationToken: cancellationToken);

			_threadTracker.TrackMessage(
				messageId: replyMessage.MessageId,
				sender: "AI",
				text: null,
				imageBase64: Convert.ToBase64String(generatedImage),
				replyToMessageId: replyMessage.MessageId
			);
		}
	}
}
