using BotNet.CommandHandlers.Art;
using Mediator;
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
using BotNet.Services.TelegramClient;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotNet.CommandHandlers.AI.OpenAI {
	public sealed class OpenAiImagePromptHandler(
		ITelegramBotClient telegramBotClient,
		ICommandQueue commandQueue,
		ITelegramMessageCache telegramMessageCache,
		OpenAiClient openAiClient,
		CommandPriorityCategorizer commandPriorityCategorizer,
		ILogger<OpenAiImageGenerationPromptHandler> logger
	) : ICommandHandler<OpenAiImagePrompt> {
		private static readonly RateLimiter VisionRateLimiter = RateLimiter.PerUserPerChat(1, TimeSpan.FromMinutes(15));
		private static readonly RateLimiter VipVisionRateLimiter = RateLimiter.PerUserPerChat(2, TimeSpan.FromMinutes(2));

		public async ValueTask<Unit> Handle(OpenAiImagePrompt imagePrompt, CancellationToken cancellationToken) {
			if (imagePrompt.Command.Sender is not VipSender
				&& imagePrompt.Command.Chat is not HomeGroupChat) {
				await telegramBotClient.SendMessage(
					chatId: imagePrompt.Command.Chat.Id,
					text: MarkdownV2Sanitizer.Sanitize("Vision tidak bisa dipakai di sini."),
					parseMode: ParseMode.MarkdownV2,
					replyParameters: new ReplyParameters {
						MessageId = imagePrompt.Command.MessageId
					},
					cancellationToken: cancellationToken
				);
						return default;
			}

			try {
				if (imagePrompt.Command.Sender is VipSender) {
					VipVisionRateLimiter.ValidateActionRate(
						chatId: imagePrompt.Command.Chat.Id,
						userId: imagePrompt.Command.Sender.Id
					);
				} else {
					VisionRateLimiter.ValidateActionRate(
						chatId: imagePrompt.Command.Chat.Id,
						userId: imagePrompt.Command.Sender.Id
					);
				}
			} catch (RateLimitExceededException exc) {
				await telegramBotClient.SendMessage(
					chatId: imagePrompt.Command.Chat.Id,
					text: $"<code>Anda terlalu banyak menggunakan vision. Coba lagi {exc.Cooldown}.</code>",
					parseMode: ParseMode.Html,
					replyParameters: new ReplyParameters {
						MessageId = imagePrompt.Command.MessageId
					},
					cancellationToken: cancellationToken
				);
						return default;
			}

			// Fire and forget
			BackgroundTask.Run(async () => {
				(string? imageBase64, string? error) = await GetImageBase64Async(
					botClient: telegramBotClient,
					fileId: imagePrompt.ImageFileId,
					cancellationToken: cancellationToken
				);

				if (error is not null) {
					await telegramBotClient.SendMessage(
						chatId: imagePrompt.Command.Chat.Id,
						text: $"<code>{error}</code>",
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters {
							MessageId = imagePrompt.Command.MessageId
						},
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
						role: message.Sender.ChatGptRole,
						text: message.Text
					)
				);

				messages.Add(
					ChatMessage.FromTextWithImageBase64("user", imagePrompt.Prompt, imageBase64!)
				);

				Message responseMessage = await telegramBotClient.SendMessage(
					chatId: imagePrompt.Command.Chat.Id,
					text: MarkdownV2Sanitizer.Sanitize("… ⏳"),
					parseMode: ParseMode.MarkdownV2,
					replyParameters: new ReplyParameters {
						MessageId = imagePrompt.Command.MessageId
					}
				);

				string response = await openAiClient.ChatAsync(
					model: "gpt-4-vision-preview",
					messages: messages,
					maxTokens: 512,
					cancellationToken: cancellationToken
				);

				// Handle image generation intent
				if (response.StartsWith("ImageGeneration:")) {
					if (imagePrompt.Command.Sender is not VipSender) {
						try {
							ArtCommandHandler.ImageGenerationRateLimiter.ValidateActionRate(imagePrompt.Command.Chat.Id, imagePrompt.Command.Sender.Id);
						} catch (RateLimitExceededException exc) {
							await telegramBotClient.SendMessage(
								chatId: imagePrompt.Command.Chat.Id,
								text: $"Anda belum mendapat giliran. Coba lagi {exc.Cooldown}.",
								parseMode: ParseMode.Html,
								replyParameters: new ReplyParameters {
									MessageId = imagePrompt.Command.MessageId
								},
								cancellationToken: cancellationToken
							);

							return;
						}
					}

					string imageGenerationPrompt = response.Substring(response.IndexOf(':') + 1).Trim();
					switch (imagePrompt.Command) {
						case { Sender: VipSender }:
							await commandQueue.DispatchAsync(
								command: new OpenAiImageGenerationPrompt(
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
							await commandQueue.DispatchAsync(
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
							await telegramBotClient.EditMessageText(
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
						parseModes: [ParseMode.MarkdownV2, ParseMode.Markdown, ParseMode.Html],
						replyMarkup: new InlineKeyboardMarkup(
							InlineKeyboardButton.WithUrl(
								text: "Generated by OpenAI GPT-4",
								url: "https://openai.com/gpt-4"
							)
						),
						cancellationToken: cancellationToken
					);
				} catch (Exception exc) {
					logger.LogError(exc, null);
					await telegramBotClient.EditMessageText(
						chatId: imagePrompt.Command.Chat.Id,
						messageId: responseMessage.MessageId,
						text: "😵",
						parseMode: ParseMode.Html,
						cancellationToken: cancellationToken
					);
					return;
				}

				// Track thread
				telegramMessageCache.Add(
					message: AiResponseMessage.FromMessage(
						message: responseMessage,
						replyToMessage: imagePrompt.Command,
						callSign: imagePrompt.CallSign,
						commandPriorityCategorizer: commandPriorityCategorizer
					)
				);
			}, logger);

			return default;
		}

		private static async Task<(string? ImageBase64, string? Error)> GetImageBase64Async(ITelegramBotClient botClient, string fileId, CancellationToken cancellationToken) {
			// Download photo
			using MemoryStream originalImageStream = new();
			await botClient.GetInfoAndDownloadFile(
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
