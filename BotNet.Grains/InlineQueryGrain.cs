using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using BotNet.Services.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace BotNet.Grains {
	public class InlineQueryGrain : Grain, IInlineQueryGrain {
		private readonly IServiceProvider _serviceProvider;
		private ImmutableList<InlineQueryResult>? _results;
		private DateTime? _lastPopulated;

		public InlineQueryGrain(
			IServiceProvider serviceProvider
		) {
			_serviceProvider = serviceProvider;
		}

		public async Task<ImmutableList<InlineQueryResult>> GetResultsAsync(string query, long userId) {
			if (_results != null
				&& _lastPopulated.HasValue
				&& DateTime.Now.Subtract(_lastPopulated.Value).TotalMinutes < 1) {
				return _results;
			}

			List<Task<ImmutableList<InlineQueryResult>>> resultTasks = new();

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
						.GetRandomJokesAsync()
						.ContinueWith(task => task.Result.Select(dadJoke => new InlineQueryResultPhoto(dadJoke.Id, dadJoke.Url, dadJoke.Url)).ToImmutableList<InlineQueryResult>())
				);
			}

			if (query.Length >= 3) {
				resultTasks.Add(
					GrainFactory
						.GetGrain<IDuckDuckGoGrain>(query)
						.SearchAsync()
						.ContinueWith(task => {
							string? hostName = _serviceProvider.GetRequiredService<IOptions<HostingOptions>>().Value.HostName;
							return task.Result.Select(resultItem => new InlineQueryResultArticle(
								id: resultItem.Url.GetHashCode(StringComparison.InvariantCultureIgnoreCase).ToString(),
								title: resultItem.Title,
								inputMessageContent: new InputTextMessageContent($"<a href=\"{resultItem.Url}\">{WebUtility.HtmlEncode(resultItem.Title)}</a>\n<pre>{resultItem.UrlText}</pre>\n\n{WebUtility.HtmlEncode(resultItem.Snippet)}") {
									ParseMode = ParseMode.Html
								}
							) {
								ThumbUrl = hostName is not null
									? $"{hostName}/opengraph/image?url={WebUtility.UrlEncode(resultItem.Url)}"
									: null,
								Url = resultItem.Url,
								Description = WebUtility.HtmlEncode(resultItem.Snippet)
							}).ToImmutableList<InlineQueryResult>();
						})
				);
			}

			if (query.Length >= 3) {
				resultTasks.Add(
					GrainFactory
						.GetGrain<ITenorGrain>(query)
						.SearchGifsAsync()
						.ContinueWith(task => task.Result.Select(gif => new InlineQueryResultGif(gif.Id, gif.Url, gif.PreviewUrl)).ToImmutableList<InlineQueryResult>())
				);
			}

			_results = (await Task.WhenAll(resultTasks)).SelectMany(results => results).ToImmutableList();
			_lastPopulated = DateTime.Now;

			return _results;
		}
	}
}
