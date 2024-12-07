using System;
using System.Collections.Generic;
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
	public sealed class OpenAiStreamingClient(
		IServiceProvider serviceProvider,
		ILogger<OpenAiStreamingClient> logger
	) : IDisposable {
		private IServiceScope? _danglingServiceScope;
		private bool _disposedValue;

		public async Task StreamChatAsync(
			string model,
			IEnumerable<ChatMessage> messages,
			int maxTokens,
			string callSign,
			long chatId,
			int replyToMessageId
		) {
			IServiceScope serviceScope = serviceProvider.CreateScope();
			_danglingServiceScope = serviceScope;
			OpenAiClient openAiClient = serviceScope.ServiceProvider.GetRequiredService<OpenAiClient>();
			ITelegramBotClient telegramBotClient = serviceScope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

			IAsyncEnumerable<(string Result, bool Stop)> enumerable = openAiClient.StreamChatAsync(
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
					logger.LogError(exc, null);
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

				Message completeMessage = await telegramBotClient.SendMessage(
					chatId: chatId,
					text: MarkdownV2Sanitizer.Sanitize(lastResult),
					parseMode: ParseMode.MarkdownV2,
					replyParameters: new ReplyParameters {
						MessageId = replyToMessageId
					}
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
			Message incompleteMessage = await telegramBotClient.SendMessage(
				chatId: chatId,
				text: MarkdownV2Sanitizer.Sanitize(lastResult ?? "") + "… ⏳", // ellipsis, nbsp, hourglass emoji
				parseMode: ParseMode.MarkdownV2,
				replyParameters: new ReplyParameters {
					MessageId = replyToMessageId
				}
			);

			// Continue streaming in the background
			_ = Task.Run(async () => {
				using CancellationTokenSource cts = new();

				_ = Task.Run(async () => {
					try {
						while (!downstreamTask.IsCompleted) {
							// ReSharper disable once AccessToDisposedClosure
							await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);

							if (lastSent != lastResult) {
								lastSent = lastResult!;
								try {
									await telegramBotClient.EditMessageText(
										chatId: chatId,
										messageId: incompleteMessage.MessageId,
										text: MarkdownV2Sanitizer.Sanitize(lastResult ?? "") + "… ⏳", // ellipsis, nbsp, hourglass emoji
										parseMode: ParseMode.MarkdownV2,
										// ReSharper disable once AccessToDisposedClosure
										cancellationToken: cts.Token
									);
								} catch (Exception exc) when (exc is not OperationCanceledException) {
									// Message might be deleted
									logger.LogError(exc, null);
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
						await telegramBotClient.EditMessageText(
							chatId: chatId,
							messageId: incompleteMessage.MessageId,
							text: MarkdownV2Sanitizer.Sanitize(lastResult ?? ""),
							parseMode: ParseMode.MarkdownV2,
							cancellationToken: cts.Token
						);
					} catch (Exception exc) {
						logger.LogError(exc, null);
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

				await cts.CancelAsync();
				serviceScope.Dispose();
			});
		}

		private void Dispose(bool disposing) {
			if (!_disposedValue) {
				if (disposing) {
					// dispose managed state (managed objects)
					_danglingServiceScope?.Dispose();
				}

				// set large fields to null
				_danglingServiceScope = null;
				_disposedValue = true;
			}
		}

		public void Dispose() {
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
		}
	}
}
