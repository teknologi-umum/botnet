using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.Controllers {
	public abstract class BotControllerBase {
		private readonly ITelegramBotClient _botClient;
		private readonly ILogger<BotControllerBase> _logger;
		private readonly Message _currentMessage;

		protected async Task<Message> ReplyMarkdownAsync(string text, CancellationToken cancellationToken = default) {
			return await _botClient.SendTextMessageAsync(
				chatId: _currentMessage.Chat.Id,
				text: text,
				parseMode: ParseMode.Markdown,
				replyToMessageId: _currentMessage.MessageId,
				cancellationToken: cancellationToken
			);
		}

		protected async Task<Message> ReplyPhotoAsync(byte[] photo, string? caption = null, CancellationToken cancellationToken = default) {
			using MemoryStream photoStream = new(photo);
			return await _botClient.SendPhotoAsync(
				chatId: _currentMessage.Chat.Id,
				photo: new InputFileStream(photoStream, "photo.png"),
				caption: caption,
				replyToMessageId: _currentMessage.MessageId,
				cancellationToken: cancellationToken
			);
		}

		protected async Task TryDeleteMessageAsync(Message message, CancellationToken cancellationToken = default) {
			try {
				await _botClient.DeleteMessageAsync(
					chatId: message.Chat.Id,
					messageId: message.MessageId,
					cancellationToken: cancellationToken
				);
			} catch (OperationCanceledException) {
				throw;
			} catch (Exception exc) {
				_logger.LogError(exc, "Failed to delete message {MessageId}", message.MessageId);
			}
		}
	}
}
