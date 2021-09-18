using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using BotNet.Services.Tenor;
using Orleans;
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

		public async Task<ImmutableList<InlineQueryResultBase>> GetResultsAsync() {
			if (_results is null) {
				string keyword = this.GetPrimaryKeyString();
				List<Task<ImmutableList<InlineQueryResultBase>>> resultTasks = new();
				if (keyword?.Trim() is { Length: >= 3 } gifQuery) {
					resultTasks.Add(
						_tenorClient.SearchGifsAsync(keyword, CancellationToken.None)
							.ContinueWith(task => task.Result.Select(gif => new InlineQueryResultGif(gif.Id, gif.Url, gif.PreviewUrl)).ToImmutableList<InlineQueryResultBase>())
					);
				}
				_results = (await Task.WhenAll(resultTasks)).SelectMany(results => results).ToImmutableList();
			}
			return _results;
		}
	}
}
