﻿using BotNet.CommandHandlers.AI.RateLimit;
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
using BotNet.Services.TelegramClient;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotNet.CommandHandlers.AI.OpenAI {
	public sealed class OpenAiTextPromptHandler(
		ITelegramBotClient telegramBotClient,
		ICommandQueue commandQueue,
		ITelegramMessageCache telegramMessageCache,
		OpenAiClient openAiClient,
		CommandPriorityCategorizer commandPriorityCategorizer,
		ILogger<OpenAiTextPromptHandler> logger
	) : ICommandHandler<OpenAiTextPrompt> {
		internal static readonly RateLimiter ChatRateLimiter = RateLimiter.PerUserPerChat(5, TimeSpan.FromMinutes(15));

		public Task Handle(OpenAiTextPrompt textPrompt, CancellationToken cancellationToken) {
			if (textPrompt.Command.Chat is GroupChat) {
				try {
					AiRateLimiters.GroupChatRateLimiter.ValidateActionRate(
						chatId: textPrompt.Command.Chat.Id,
						userId: textPrompt.Command.Sender.Id
					);
				} catch (RateLimitExceededException exc) {
					return telegramBotClient.SendMessage(
						chatId: textPrompt.Command.Chat.Id,
						text: $"<code>Anda terlalu banyak memanggil AI. Coba lagi {exc.Cooldown} atau lanjutkan di private chat.</code>",
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters {
							MessageId = textPrompt.Command.MessageId
						},
						replyMarkup: new InlineKeyboardMarkup(
							InlineKeyboardButton.WithUrl("Private chat 💬", "t.me/TeknumBot")
						),
						cancellationToken: cancellationToken
					);
				}
			} else {
				try {
					ChatRateLimiter.ValidateActionRate(
						chatId: textPrompt.Command.Chat.Id,
						userId: textPrompt.Command.Sender.Id
					);
				} catch (RateLimitExceededException exc) {
					return telegramBotClient.SendMessage(
						chatId: textPrompt.Command.Chat.Id,
						text: $"<code>Anda terlalu banyak memanggil AI. Coba lagi {exc.Cooldown}.</code>",
						parseMode: ParseMode.Html,
						replyParameters: new ReplyParameters {
							MessageId = textPrompt.Command.MessageId
						},
						cancellationToken: cancellationToken
					);
				}
			}

			// Fire and forget
			Task.Run(async () => {
				List<ChatMessage> messages = [
					ChatMessage.FromText("system", "The following is a conversation with an AI assistant. The assistant is helpful, creative, direct, concise, and always get to the point. When user asks for an image to be generated, the AI assistant should respond with \"ImageGeneration:\" followed by comma separated list of features to be expected from the generated image.")
				];

				messages.AddRange(
					from message in textPrompt.Thread.Take(10).Reverse()
					select ChatMessage.FromText(
						role: message.Sender.ChatGptRole,
						text: message.Text
					)
				);

				messages.Add(
					ChatMessage.FromText("user", textPrompt.Prompt)
				);

				Message responseMessage = await telegramBotClient.SendMessage(
					chatId: textPrompt.Command.Chat.Id,
					text: MarkdownV2Sanitizer.Sanitize("… ⏳"),
					parseMode: ParseMode.MarkdownV2,
					replyParameters: new ReplyParameters {
						MessageId = textPrompt.Command.MessageId
					}
				);

				string response = await openAiClient.ChatAsync(
					model: textPrompt switch {
						{ Command: { Sender: VipSender } or { Chat: HomeGroupChat } } => "gpt-4-1106-preview",
						_ => "gpt-3.5-turbo"
					},
					messages: messages,
					maxTokens: 512,
					cancellationToken: cancellationToken
				);

				// Handle image generation intent
				if (response.StartsWith("ImageGeneration:")) {
					if (textPrompt.Command.Sender is not VipSender) {
						try {
							ArtCommandHandler.ImageGenerationRateLimiter.ValidateActionRate(textPrompt.Command.Chat.Id, textPrompt.Command.Sender.Id);
						} catch (RateLimitExceededException exc) {
							await telegramBotClient.SendMessage(
								chatId: textPrompt.Command.Chat.Id,
								text: $"Anda belum mendapat giliran. Coba lagi {exc.Cooldown}.",
								parseMode: ParseMode.Html,
								replyParameters: new ReplyParameters {
									MessageId = textPrompt.Command.MessageId
								},
								cancellationToken: cancellationToken
							);
							return;
						}
					}

					string imageGenerationPrompt = response.Substring(response.IndexOf(':') + 1).Trim();
					switch (textPrompt.Command) {
						case { Sender: VipSender }:
							await commandQueue.DispatchAsync(
								command: new OpenAiImageGenerationPrompt(
									callSign: textPrompt.CallSign,
									prompt: imageGenerationPrompt,
									promptMessageId: textPrompt.Command.MessageId,
									responseMessageId: new(responseMessage.MessageId),
									chat: textPrompt.Command.Chat,
									sender: textPrompt.Command.Sender
								)
							);
							break;
						case { Chat: HomeGroupChat }:
							await commandQueue.DispatchAsync(
								command: new StabilityTextToImagePrompt(
									callSign: textPrompt.CallSign,
									prompt: imageGenerationPrompt,
									promptMessageId: textPrompt.Command.MessageId,
									responseMessageId: new(responseMessage.MessageId),
									chat: textPrompt.Command.Chat,
									sender: textPrompt.Command.Sender
								)
							);
							break;
						default:
							await telegramBotClient.EditMessageText(
								chatId: textPrompt.Command.Chat.Id,
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
						chatId: textPrompt.Command.Chat.Id,
						messageId: responseMessage.MessageId,
						text: response,
						parseModes: [ParseMode.MarkdownV2, ParseMode.Markdown, ParseMode.Html],
						replyMarkup: new InlineKeyboardMarkup(
							InlineKeyboardButton.WithUrl(
								text: textPrompt switch {
									{ Command: { Sender: VipSender } or { Chat: HomeGroupChat } } => "Generated by OpenAI GPT-4",
									_ => "Generated by OpenAI GPT-3.5 Turbo"
								},
								url: "https://openai.com/gpt-4"
							)
						),
						cancellationToken: cancellationToken
					);
				} catch (Exception exc) {
					logger.LogError(exc, null);
					await telegramBotClient.EditMessageText(
						chatId: textPrompt.Command.Chat.Id,
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
						replyToMessage: textPrompt.Command,
						callSign: textPrompt.CallSign,
						commandPriorityCategorizer: commandPriorityCategorizer
					)
				);
			});

			return Task.CompletedTask;
		}
	}
}
