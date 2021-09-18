using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using BotNet.Services.Tenor;
using Orleans;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace BotNet.Grains {
	public class InlineQueryGrain : Grain, IInlineQueryGrain {
		private readonly TenorClient _tenorClient;
		private ImmutableList<InlineQueryResultBase>? _results;

		public InlineQueryGrain(
			TenorClient tenorClient
		) {
			_tenorClient = tenorClient;
		}

		public async Task<ImmutableList<InlineQueryResultBase>> GetResultsAsync(InlineQuery inlineQuery) {
			if (_results is null) {
				string keyword = this.GetPrimaryKeyString();
				List<Task<ImmutableList<InlineQueryResultBase>>> resultTasks = new();

				if (keyword.Split(' ').FirstOrDefault() is "joke" or "jokes" or "dad" or "bapak" or "bapack" or "dadjoke" or "dadjokes" or "bapak2" or "bapack2") {
					resultTasks.Add(
						GrainFactory
							.GetGrain<IDadJokeGrain>(inlineQuery.From.Id % 10)
							.GetRandomJokesAsync()
							.ContinueWith(task => task.Result.Select(dadJoke => new InlineQueryResultPhoto(dadJoke.Id, dadJoke.Url, dadJoke.Url)).ToImmutableList<InlineQueryResultBase>())
					);
				}

				if (keyword.Length >= 3) {
					resultTasks.Add(
						_tenorClient
							.SearchGifsAsync(keyword, CancellationToken.None)
							.ContinueWith(task => task.Result.Select(gif => new InlineQueryResultGif(gif.Id, gif.Url, gif.PreviewUrl)).ToImmutableList<InlineQueryResultBase>())
					);
				}

				_results = (await Task.WhenAll(resultTasks)).SelectMany(results => results).ToImmutableList();
			}
			return _results;
		}
	}
}
