﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.MarkdownV2;
using BotNet.Services.OpenAI.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Services.OpenAI {
	public sealed class OpenAIStreamingClient(
		IServiceProvider serviceProvider,
		ILogger<OpenAIStreamingClient> logger
	) {
		private readonly IServiceProvider _serviceProvider = serviceProvider;
		private readonly ILogger<OpenAIStreamingClient> _logger = logger;

		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Manually managed")]
		public async Task StreamChatAsync(
			string model,
			IEnumerable<ChatMessage> messages,
			int maxTokens,
			string callSign,
			long chatId,
			int replyToMessageId
		) {
			IServiceScope serviceScope = _serviceProvider.CreateScope();
			OpenAIClient openAIClient = serviceScope.ServiceProvider.GetRequiredService<OpenAIClient>();
			ITelegramBotClient telegramBotClient = serviceScope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

			IAsyncEnumerable<(string Result, bool Stop)> enumerable = openAIClient.StreamChatAsync(
				model: model,
				messages: messages,
				maxTokens: maxTokens,
				cancellationToken: CancellationToken.None
			);

			string? lastResult = null;

			// Task for continuously consuming the stream
			Task downstreamTask = Task.Run(async () => {
				try {
					await foreach ((string result, bool stop) in enumerable) {
						lastResult = result;

						if (stop) {
							break;
						}
					}
				} catch (Exception exc) {
					_logger.LogError(exc, null);
				}
			});

			// Wait until the stream is stopped or 2 seconds have passed
			await Task.WhenAny(
				downstreamTask,
				Task.Delay(TimeSpan.FromSeconds(2))
			);

			// If downstream task is completed, send the last result
			if (downstreamTask.IsCompletedSuccessfully) {
				if (lastResult is null) return;

				Message completeMessage = await telegramBotClient.SendTextMessageAsync(
					chatId: chatId,
					text: MarkdownV2Sanitizer.Sanitize(lastResult),
					parseMode: ParseMode.MarkdownV2,
					replyToMessageId: replyToMessageId
				);

				// Track thread
				ThreadTracker threadTracker = serviceScope.ServiceProvider.GetRequiredService<ThreadTracker>();
				threadTracker.TrackMessage(
					messageId: completeMessage.MessageId,
					sender: callSign,
					text: lastResult,
					imageBase64: null,
					replyToMessageId: replyToMessageId
				);

				serviceScope.Dispose();
				return;
			}

			// Otherwise, send incomplete result and continue streaming
			string lastSent = lastResult ?? "";
			Message incompleteMessage = await telegramBotClient.SendTextMessageAsync(
				chatId: chatId,
				text: MarkdownV2Sanitizer.Sanitize(lastResult ?? "") + "… ⏳", // ellipsis, nbsp, hourglass emoji
				parseMode: ParseMode.MarkdownV2,
				replyToMessageId: replyToMessageId
			);

			// Continue streaming in the background
			_ = Task.Run(async () => {
				using CancellationTokenSource cts = new();

				_ = Task.Run(async () => {
					try {
						while (!downstreamTask.IsCompleted) {
							await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);

							if (lastSent != lastResult) {
								lastSent = lastResult!;
								try {
									await telegramBotClient.EditMessageTextAsync(
										chatId: chatId,
										messageId: incompleteMessage.MessageId,
										text: MarkdownV2Sanitizer.Sanitize(lastResult ?? "") + "… ⏳", // ellipsis, nbsp, hourglass emoji
										parseMode: ParseMode.MarkdownV2,
										cancellationToken: cts.Token
									);
								} catch (Exception exc) when (exc is not OperationCanceledException) {
									// Message might be deleted
									_logger.LogError(exc, null);
									break;
								}
							}
						}
					} catch (OperationCanceledException) {
						// Exit gracefully
					}
				});

				await downstreamTask;

				try {
					// Finalize message
					try {
						await telegramBotClient.EditMessageTextAsync(
							chatId: chatId,
							messageId: incompleteMessage.MessageId,
							text: MarkdownV2Sanitizer.Sanitize(lastResult ?? ""),
							parseMode: ParseMode.MarkdownV2,
							cancellationToken: cts.Token
						);
					} catch (Exception exc) {
						_logger.LogError(exc, null);
						throw;
					}

					// Track thread
					ThreadTracker threadTracker = serviceScope.ServiceProvider.GetRequiredService<ThreadTracker>();
					threadTracker.TrackMessage(
						messageId: incompleteMessage.MessageId,
						sender: callSign,
						text: lastResult!,
						imageBase64: null,
						replyToMessageId: replyToMessageId
					);
				} catch {
					// Message might be deleted, suppress exception
				}

				cts.Cancel();
				serviceScope.Dispose();
			});
		}
	}
}
