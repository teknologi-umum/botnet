using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using BotNet.Services.Tenor;
using Orleans;
using Telegram.Bot.Types.InlineQueryResults;

namespace BotNet.Grains {
	public class InlineQueryGrain : Grain, IInlineQueryGrain {
		private readonly TenorClient _tenorClient;
		private ImmutableList<InlineQueryResultBase>? _results;
		private DateTime? _lastPopulated;

		public InlineQueryGrain(
			TenorClient tenorClient
		) {
			_tenorClient = tenorClient;
		}

		public async Task<ImmutableList<InlineQueryResultBase>> GetResultsAsync(string query, long userId) {
			if (_results != null
				&& _lastPopulated.HasValue
				&& DateTime.Now.Subtract(_lastPopulated.Value).TotalMinutes < 1) {
				return _results;
			}

			List<Task<ImmutableList<InlineQueryResultBase>>> resultTasks = new();

			if (query.Split(' ').FirstOrDefault() is "joke" or "jokes" or "dad" or "bapak" or "bapack" or "dadjoke" or "dadjokes" or "bapak2" or "bapack2") {
				resultTasks.Add(
					GrainFactory
						.GetGrain<IDadJokeGrain>(userId % 10)
						.GetRandomJokesAsync()
						.ContinueWith(task => task.Result.Select(dadJoke => new InlineQueryResultPhoto(dadJoke.Id, dadJoke.Url, dadJoke.Url)).ToImmutableList<InlineQueryResultBase>())
				);
			}

			if (query.Length >= 3) {
				resultTasks.Add(
					GrainFactory
						.GetGrain<ITenorGrain>(query)
						.SearchGifsAsync()
						.ContinueWith(task => task.Result.Select(gif => new InlineQueryResultGif(gif.Id, gif.Url, gif.PreviewUrl)).ToImmutableList<InlineQueryResultBase>())
				);
			}

			_results = (await Task.WhenAll(resultTasks)).SelectMany(results => results).ToImmutableList();
			_lastPopulated = DateTime.Now;

			return _results;
		}
	}
}
