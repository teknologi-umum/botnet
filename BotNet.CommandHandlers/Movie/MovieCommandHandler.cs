using System;
using Mediator;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Commands.Movie;
using BotNet.Services.MarkdownV2;
using BotNet.Services.OMDb;
using BotNet.Services.RateLimit;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotNet.CommandHandlers.Movie {
	public sealed class MovieCommandHandler(
		ITelegramBotClient telegramBotClient,
		OmdbClient omdbClient,
		ILogger<MovieCommandHandler> logger
	) : ICommandHandler<MovieCommand> {
		private static readonly RateLimiter RateLimiter = RateLimiter.PerUserPerChat(3, TimeSpan.FromMinutes(2));

		public async ValueTask<Unit> Handle(MovieCommand command, CancellationToken cancellationToken) {
			try {
				RateLimiter.ValidateActionRate(
					chatId: command.Chat.Id,
					userId: command.Sender.Id
				);
			} catch (RateLimitExceededException exc) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"<code>Coba lagi {exc.Cooldown}</code>",
					parseMode: ParseMode.Html,
					cancellationToken: cancellationToken
				);
				return default;
			}

			try {
				OmdbResponse movie = await omdbClient.GetByTitleAsync(command.Title, cancellationToken);

				// Build the message text
				StringBuilder messageBuilder = new();
				messageBuilder.AppendLine($"*{MarkdownV2Sanitizer.Sanitize(movie.Title ?? "Unknown")}* \\({MarkdownV2Sanitizer.Sanitize(movie.Year ?? "N/A")}\\)");
				messageBuilder.AppendLine();

				if (!string.IsNullOrWhiteSpace(movie.Genre)) {
					messageBuilder.AppendLine($"🎬 *Genre:* {MarkdownV2Sanitizer.Sanitize(movie.Genre)}");
				}

				if (!string.IsNullOrWhiteSpace(movie.Runtime)) {
					messageBuilder.AppendLine($"⏱️ *Runtime:* {MarkdownV2Sanitizer.Sanitize(movie.Runtime)}");
				}

				if (movie.Type == "series" && !string.IsNullOrWhiteSpace(movie.TotalSeasons)) {
					messageBuilder.AppendLine($"📺 *Seasons:* {MarkdownV2Sanitizer.Sanitize(movie.TotalSeasons)}");
				}

				// Ratings
				if (movie.Ratings?.Length > 0) {
					messageBuilder.AppendLine();
					messageBuilder.AppendLine("*Ratings:*");
					foreach (OmdbRating rating in movie.Ratings) {
						string source = rating.Source switch {
							"Internet Movie Database" => "⭐ IMDb",
							"Rotten Tomatoes" => "🍅 Rotten Tomatoes",
							"Metacritic" => "📊 Metacritic",
							_ => rating.Source ?? "Unknown"
						};
						messageBuilder.AppendLine($"{MarkdownV2Sanitizer.Sanitize(source)}: {MarkdownV2Sanitizer.Sanitize(rating.Value ?? "N/A")}");
					}
				} else if (!string.IsNullOrWhiteSpace(movie.ImdbRating)) {
					messageBuilder.AppendLine();
					messageBuilder.AppendLine($"⭐ *IMDb:* {MarkdownV2Sanitizer.Sanitize(movie.ImdbRating)}/10");
				}

				// Plot
				if (!string.IsNullOrWhiteSpace(movie.Plot) && movie.Plot != "N/A") {
					messageBuilder.AppendLine();
					messageBuilder.AppendLine($"_{MarkdownV2Sanitizer.Sanitize(movie.Plot)}_");
				}

				// Director and Cast
				if (!string.IsNullOrWhiteSpace(movie.Director) && movie.Director != "N/A") {
					messageBuilder.AppendLine();
					messageBuilder.AppendLine($"🎥 *Director:* {MarkdownV2Sanitizer.Sanitize(movie.Director)}");
				}

				if (!string.IsNullOrWhiteSpace(movie.Actors) && movie.Actors != "N/A") {
					messageBuilder.AppendLine($"🎭 *Cast:* {MarkdownV2Sanitizer.Sanitize(movie.Actors)}");
				}

				// Try to send with poster image
				if (!string.IsNullOrWhiteSpace(movie.Poster) && movie.Poster != "N/A") {
					try {
						byte[] posterImage = await omdbClient.GetPosterImageAsync(movie.Poster, cancellationToken);
						await telegramBotClient.SendPhoto(
							chatId: command.Chat.Id,
							photo: InputFile.FromStream(new System.IO.MemoryStream(posterImage)),
							caption: messageBuilder.ToString(),
							parseMode: ParseMode.MarkdownV2,
							cancellationToken: cancellationToken
						);
					} catch {
						// If poster fails, send text only
						await telegramBotClient.SendMessage(
							chatId: command.Chat.Id,
							text: messageBuilder.ToString(),
							parseMode: ParseMode.MarkdownV2,
							cancellationToken: cancellationToken
						);
					}
				} else {
					await telegramBotClient.SendMessage(
						chatId: command.Chat.Id,
						text: messageBuilder.ToString(),
						parseMode: ParseMode.MarkdownV2,
						cancellationToken: cancellationToken
					);
				}
			} catch (InvalidOperationException exc) {
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: $"❌ {MarkdownV2Sanitizer.Sanitize(exc.Message)}",
					parseMode: ParseMode.MarkdownV2,
					replyParameters: new ReplyParameters {
						MessageId = command.MessageId
					},
					cancellationToken: cancellationToken
				);
			} catch (Exception exc) {
				logger.LogError(exc, "Failed to get movie information");
				await telegramBotClient.SendMessage(
					chatId: command.Chat.Id,
					text: "❌ Failed to retrieve movie information\\. Please try again later\\.",
					parseMode: ParseMode.MarkdownV2,
					replyParameters: new ReplyParameters {
						MessageId = command.MessageId
					},
					cancellationToken: cancellationToken
				);
			}
	return default;
		}
	}
}
