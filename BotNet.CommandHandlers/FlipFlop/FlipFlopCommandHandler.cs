using BotNet.Commands.FlipFlop;
using BotNet.Services.ImageFlip;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = Telegram.Bot.Types.File;

namespace BotNet.CommandHandlers.FlipFlop {
	internal sealed class FlipFlopCommandHandler(
		ITelegramBotClient telegramBotClient
	) : ICommandHandler<FlipFlopCommand> {
		private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;

		public async Task Handle(FlipFlopCommand command, CancellationToken cancellationToken) {
			// Download original image
			using MemoryStream originalImageStream = new();
			File fileInfo = await _telegramBotClient.GetInfoAndDownloadFile(
				fileId: command.ImageFileId,
				destination: originalImageStream,
				cancellationToken: cancellationToken
			);

			// Process image
			byte[] resultImage;
			switch (command.Command) {
				case "/flip":
					resultImage = Flipper.FlipImage(originalImageStream.ToArray());
					break;
				case "/flop":
					resultImage = Flipper.FlopImage(originalImageStream.ToArray());
					break;
				case "/flep":
					resultImage = Flipper.FlepImage(originalImageStream.ToArray());
					break;
				case "/flap":
					resultImage = Flipper.FlapImage(originalImageStream.ToArray());
					break;
				default:
					throw new InvalidOperationException($"Unknown command: {command.Command}");
			}

			// Send result image
			using MemoryStream resultImageStream = new(resultImage);
			await _telegramBotClient.SendPhoto(
				chatId: command.Chat.Id,
				photo: new InputFileStream(resultImageStream, new string(fileInfo.FileId.Reverse().ToArray()) + ".png"),
				replyParameters: new ReplyParameters { MessageId = command.ImageMessageId },
				cancellationToken: cancellationToken
			);
		}
	}
}
