using BotNet.CommandHandlers.Art;
using BotNet.Commands;
using BotNet.Commands.AI.OpenAI;
using BotNet.Commands.AI.Stability;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.CommandPrioritization;
using BotNet.Commands.SenderAggregate;
using BotNet.Services.MarkdownV2;
using BotNet.Services.OpenAI;
using BotNet.Services.OpenAI.Models;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.AI.OpenAI {
	public sealed class OpenAIImagePromptHandler(
		ITelegramBotClient telegramBotClient,
		ICommandQueue commandQueue,
		ITelegramMessageCache telegramMessageCache,
		OpenAIClient openAIClient,
		CommandPriorityCategorizer commandPriorityCategorizer,
		ILogger<OpenAIImageGenerationPromptHandler> logger
	) : ICommandHandler<OpenAIImagePrompt> {
		internal static readonly RateLimiter VISION_RATE_LIMITER = RateLimiter.PerUserPerChat(1, TimeSpan.FromMinutes(15));
		internal static readonly RateLimiter VIP_VISION_RATE_LIMITER = RateLimiter.PerUserPerChat(2, TimeSpan.FromMinutes(5));

		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
		private readonly ICommandQueue _commandQueue = commandQueue;
		private readonly ITelegramMessageCache _telegramMessageCache = telegramMessageCache;
		private readonly OpenAIClient _openAIClient = openAIClient;
		private readonly CommandPriorityCategorizer _commandPriorityCategorizer = commandPriorityCategorizer;
		private readonly ILogger<OpenAIImageGenerationPromptHandler> _logger = logger;

		public Task Handle(OpenAIImagePrompt imagePrompt, CancellationToken cancellationToken) {
			if (imagePrompt.Command.Sender is not VIPSender
				&& imagePrompt.Command.Chat is not HomeGroupChat) {
				return _telegramBotClient.SendTextMessageAsync(
					chatId: imagePrompt.Command.Chat.Id,
					text: MarkdownV2Sanitizer.Sanitize("Vision tidak bisa dipakai di sini."),
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: imagePrompt.Command.MessageId,
					cancellationToken: cancellationToken
				);
			}

			try {
				if (imagePrompt.Command.Sender is VIPSender) {
					VIP_VISION_RATE_LIMITER.ValidateActionRate(
						chatId: imagePrompt.Command.Chat.Id,
						userId: imagePrompt.Command.Sender.Id
					);
				} else {
					VISION_RATE_LIMITER.ValidateActionRate(
						chatId: imagePrompt.Command.Chat.Id,
						userId: imagePrompt.Command.Sender.Id
					);
				}
			} catch (RateLimitExceededException exc) {
				return _telegramBotClient.SendTextMessageAsync(
					chatId: imagePrompt.Command.Chat.Id,
					text: $"<code>Anda terlalu banyak menggunakan vision. Coba lagi {exc.Cooldown}.</code>",
					parseMode: ParseMode.Html,
					replyToMessageId: imagePrompt.Command.MessageId,
					cancellationToken: cancellationToken
				);
			}

			// Fire and forget
			Task.Run(async () => {
				(string? imageBase64, string? error) = await GetImageBase64Async(
					botClient: _telegramBotClient,
					fileId: imagePrompt.ImageFileId,
					cancellationToken: cancellationToken
				);

				if (error is not null) {
					await _telegramBotClient.SendTextMessageAsync(
						chatId: imagePrompt.Command.Chat.Id,
						text: $"<code>{error}</code>",
						parseMode: ParseMode.Html,
						replyToMessageId: imagePrompt.Command.MessageId,
						cancellationToken: cancellationToken
					);
					return;
				}

				List<ChatMessage> messages = [
					ChatMessage.FromText("system", "The following is a conversation with an AI assistant. The assistant is helpful, creative, direct, concise, and always get to the point. When user asks for an image to be generated, the AI assistant should respond with \"ImageGeneration:\" followed by comma separated list of features to be expected from the generated image.")
				];

				messages.AddRange(
					from message in imagePrompt.Thread.Take(10).Reverse()
					select ChatMessage.FromText(
						role: message.Sender.ChatGPTRole,
						text: message.Text
					)
				);

				messages.Add(
					ChatMessage.FromTextWithImageBase64("user", imagePrompt.Prompt, imageBase64!)
				);

				Message responseMessage = await _telegramBotClient.SendTextMessageAsync(
					chatId: imagePrompt.Command.Chat.Id,
					text: MarkdownV2Sanitizer.Sanitize("… ⏳"),
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: imagePrompt.Command.MessageId
				);

				string response = await _openAIClient.ChatAsync(
					model: "gpt-4-vision-preview",
					messages: messages,
					maxTokens: 512,
					cancellationToken: cancellationToken
				);

				// Handle image generation intent
				if (response.StartsWith("ImageGeneration:")) {
					if (imagePrompt.Command.Sender is not VIPSender) {
						try {
							ArtCommandHandler.IMAGE_GENERATION_RATE_LIMITER.ValidateActionRate(imagePrompt.Command.Chat.Id, imagePrompt.Command.Sender.Id);
						} catch (RateLimitExceededException exc) {
							await _telegramBotClient.SendTextMessageAsync(
								chatId: imagePrompt.Command.Chat.Id,
								text: $"Anda belum mendapat giliran. Coba lagi {exc.Cooldown}.",
								parseMode: ParseMode.Html,
								replyToMessageId: imagePrompt.Command.MessageId,
								cancellationToken: cancellationToken
							);
							return;
						}
					}

					string imageGenerationPrompt = response.Substring(response.IndexOf(':') + 1).Trim();
					switch (imagePrompt.Command) {
						case { Sender: VIPSender }:
							await _commandQueue.DispatchAsync(
								command: new OpenAIImageGenerationPrompt(
									callSign: imagePrompt.CallSign,
									prompt: imageGenerationPrompt,
									promptMessageId: imagePrompt.Command.MessageId,
									responseMessageId: new(responseMessage.MessageId),
									chat: imagePrompt.Command.Chat,
									sender: imagePrompt.Command.Sender
								)
							);
							break;
						case { Chat: HomeGroupChat }:
							await _commandQueue.DispatchAsync(
								command: new StabilityTextToImagePrompt(
									callSign: imagePrompt.CallSign,
									prompt: imageGenerationPrompt,
									promptMessageId: imagePrompt.Command.MessageId,
									responseMessageId: new(responseMessage.MessageId),
									chat: imagePrompt.Command.Chat,
									sender: imagePrompt.Command.Sender
								)
							);
							break;
						default:
							await _telegramBotClient.EditMessageTextAsync(
								chatId: imagePrompt.Command.Chat.Id,
								messageId: responseMessage.MessageId,
								text: MarkdownV2Sanitizer.Sanitize("Image generation tidak bisa dipakai di sini."),
								parseMode: ParseMode.MarkdownV2,
								cancellationToken: cancellationToken
							);
							break;
					}
					return;
				}

				// Finalize message
				try {
					responseMessage = await telegramBotClient.EditMessageTextAsync(
						chatId: imagePrompt.Command.Chat.Id,
						messageId: responseMessage.MessageId,
						text: MarkdownV2Sanitizer.Sanitize(response),
						parseMode: ParseMode.MarkdownV2,
						cancellationToken: cancellationToken
					);
				} catch (Exception exc) {
					_logger.LogError(exc, null);
					throw;
				}

				// Track thread
				_telegramMessageCache.Add(
					message: AIResponseMessage.FromMessage(
						message: responseMessage,
						replyToMessage: imagePrompt.Command,
						callSign: imagePrompt.CallSign,
						commandPriorityCategorizer: _commandPriorityCategorizer
					)
				);
			});

			return Task.CompletedTask;
		}

		private static async Task<(string? ImageBase64, string? Error)> GetImageBase64Async(ITelegramBotClient botClient, string fileId, CancellationToken cancellationToken) {
			// Download photo
			using MemoryStream originalImageStream = new();
			await botClient.GetInfoAndDownloadFileAsync(
				fileId: fileId,
				destination: originalImageStream,
				cancellationToken: cancellationToken);
			byte[] originalImage = originalImageStream.ToArray();

			// Limit input image to 300KB
			if (originalImage.Length > 300 * 1024) {
				return (null, "Image larger than 300KB");
			}

			// Decode image
			originalImageStream.Position = 0;
			using SKCodec codec = SKCodec.Create(originalImageStream, out SKCodecResult codecResult);
			if (codecResult != SKCodecResult.Success) {
				return (null, "Invalid image");
			}

			if (codec.EncodedFormat != SKEncodedImageFormat.Jpeg
				&& codec.EncodedFormat != SKEncodedImageFormat.Webp) {
				return (null, "Image must be compressed image");
			}
			SKBitmap bitmap = SKBitmap.Decode(codec);

			// Limit input image to 1280x1280
			if (bitmap.Width > 1280 || bitmap.Width > 1280) {
				return (null, "Image larger than 1280x1280");
			}

			// Handle stickers
			if (codec.EncodedFormat == SKEncodedImageFormat.Webp) {
				SKImage image = SKImage.FromBitmap(bitmap);
				SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 20);
				using MemoryStream jpegStream = new();
				data.SaveTo(jpegStream);

				// Encode image as base64
				return (Convert.ToBase64String(jpegStream.ToArray()), null);
			}

			// Encode image as base64
			return (Convert.ToBase64String(originalImage), null);
		}
	}
}
