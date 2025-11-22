using BotNet.Commands.QrCode;
using Mediator;
using BotNet.Commands.SenderAggregate;
using BotNet.Services.QrCode;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotNet.CommandHandlers.QrCode {
	public sealed class QrCommandHandler(
		ITelegramBotClient telegramBotClient,
		QrCodeGenerator qrCodeGenerator,
		ILogger<QrCommandHandler> logger
	) : ICommandHandler<QrCommand> {
		private static readonly RateLimiter RateLimiter = RateLimiter.PerUserPerChat(3, TimeSpan.FromMinutes(1));

		public async ValueTask<Unit> Handle(QrCommand command, CancellationToken cancellationToken) {
			// Rate limiting
			try {
				RateLimiter.ValidateActionRate(
					chatId: command.Chat.Id,
					userId: command.Sender.Id
				);
			} catch (RateLimitExceededException exc) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"Rate limit exceeded. Try again {exc.Cooldown}.",
					cancellationToken: cancellationToken
				);
				return default;
			}

			try {
				// Generate QR code
				byte[] qrCodeImage = qrCodeGenerator.GenerateQrCode(command.Url);

				// Send QR code image
				await telegramBotClient.SendPhoto(
					chatId: command.Chat.Id,
					photo: new InputFileStream(new MemoryStream(qrCodeImage), "qrcode.png"),
					caption: command.Url,
					replyParameters: new ReplyParameters {
						MessageId = command.MessageId
					},
					cancellationToken: cancellationToken
				);
			} catch (Exception exc) {
				logger.LogError(exc, "Failed to generate QR code for URL: {Url}", command.Url);
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: "Failed to generate QR code. Please try again later.",
					cancellationToken: cancellationToken
				);
			}
	return default;
		}
	}
}
