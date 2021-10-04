using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using BotNet.Services.Brainfuck;
using BotNet.Services.DuckDuckGo.Models;
using BotNet.Services.FancyText;
using BotNet.Services.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using RG.Ninja;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace BotNet.Grains {
	public class InlineQueryGrain : Grain, IInlineQueryGrain {
		private readonly IServiceProvider _serviceProvider;
		private ImmutableList<InlineQueryResult>? _results;

		public InlineQueryGrain(
			IServiceProvider serviceProvider
		) {
			_serviceProvider = serviceProvider;
		}

		public async Task<ImmutableList<InlineQueryResult>> GetResultsAsync(string query, long userId, GrainCancellationToken grainCancellationToken) {
			if (_results != null) {
				DelayDeactivation(TimeSpan.FromMinutes(1));
				return _results;
			}

			List<Task<ImmutableList<InlineQueryResult>>> resultTasks = new();

			if (query.Length >= 3) {
				resultTasks.Add(
					GrainFactory
						.GetGrain<IDuckDuckGoGrain>(query)
						.SearchAsync(grainCancellationToken)
						.ContinueWith(resultItemsTask => {
							ImmutableList<SearchResultItem> resultItems = resultItemsTask.Result;
							if (resultItems.IsEmpty) return ImmutableList<InlineQueryResult>.Empty;

							string? hostName = _serviceProvider.GetRequiredService<IOptions<HostingOptions>>().Value.HostName;

							return resultItems.Select(resultItem => new InlineQueryResultArticle(
								id: resultItem.Url.GetHashCode(StringComparison.InvariantCultureIgnoreCase).ToString(),
								title: resultItem.Title,
								inputMessageContent: new InputTextMessageContent($"\n<a href=\"{resultItem.Url}\">{WebUtility.HtmlEncode(resultItem.Title)}</a>\n<pre>{resultItem.UrlText}</pre>\n\n{WebUtility.HtmlEncode(resultItem.Snippet)}") {
									ParseMode = ParseMode.Html
								}
							) {
								ThumbUrl = hostName is not null
									? $"{hostName}/opengraph/image?url={WebUtility.UrlEncode(resultItem.Url)}"
									: resultItem.IconUrl,
								Url = resultItem.Url,
								Description = resultItem.Snippet
							}).ToImmutableList<InlineQueryResult>();
						}, grainCancellationToken.CancellationToken)
				);
			}

			if (query.Length > 0) {
				string[] fancyTexts = await Task.WhenAll(
					Enum.GetValues<FancyTextStyle>()
						.Select(style => FancyTextGenerator.GenerateAsync(query, style, grainCancellationToken.CancellationToken))
				);
				resultTasks.Add(Task.FromResult(fancyTexts.Select(fancyText => new InlineQueryResultArticle(
					id: Guid.NewGuid().ToString("N"),
					title: fancyText,
					inputMessageContent: new InputTextMessageContent(fancyText)
				)).ToImmutableList<InlineQueryResult>()));
			}

			if (query.Length > 0) {
				string brainfuck = _serviceProvider
					.GetRequiredService<BrainfuckTranspiler>()
					.TranspileBrainfuck(query);
				resultTasks.Add(
					Task.FromResult(
						ImmutableList.Create<InlineQueryResult>(
							new InlineQueryResultArticle(
								id: Guid.NewGuid().ToString("N"),
								title: brainfuck,
								inputMessageContent: new InputTextMessageContent(brainfuck)
							)
						)
					)
				);
			}

			if (query.Length is 4 or 7 && query[0] == '#' && query[1..].All(c => c is >= 'a' and <= 'f' || c is >= 'A' and <= 'F' || char.IsDigit(c))) {
				HostingOptions hostingOptions = _serviceProvider.GetRequiredService<IOptions<HostingOptions>>().Value;
				string url = $"https://{hostingOptions.HostName}/renderer/color?name={WebUtility.UrlEncode(query)}";
				resultTasks.Add(Task.FromResult(ImmutableList.Create<InlineQueryResult>(
					new InlineQueryResultPhoto($"color{query[1..]}", url, url) {
						Title = query,
						Description = query,
						PhotoWidth = 200,
						PhotoHeight = 200
					}
				)));
			}

			if (query.Split(' ').FirstOrDefault() is "joke" or "jokes" or "dad" or "bapak" or "bapack" or "dadjoke" or "dadjokes" or "bapak2" or "bapack2") {
				resultTasks.Add(
					GrainFactory
						.GetGrain<IDadJokeGrain>(userId % 10)
						.GetRandomJokesAsync(grainCancellationToken)
						.ContinueWith(task => task.Result.Select(dadJoke => new InlineQueryResultPhoto(dadJoke.Id, dadJoke.Url, dadJoke.Url) {
							Title = "Random dad joke",
							Description = "Jokes bapack-bapack yang dipilih khusus buat kamu"
						}).ToImmutableList<InlineQueryResult>())
				);
			}

			if (query.StartsWith("gif ", StringComparison.InvariantCultureIgnoreCase, out string? gifQuery)) {
				resultTasks.Add(
					GrainFactory
						.GetGrain<ITenorGrain>(gifQuery)
						.SearchGifsAsync(grainCancellationToken)
						.ContinueWith(task => task.Result.Select(gif => new InlineQueryResultGif(gif.Id, gif.Url, gif.PreviewUrl)).ToImmutableList<InlineQueryResult>())
				);
			}

			_results = (await Task.WhenAll(resultTasks)).SelectMany(results => results).ToImmutableList();

			return _results;
		}
	}
}
