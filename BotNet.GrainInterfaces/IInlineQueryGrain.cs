using System.Collections.Immutable;
using System.Threading.Tasks;
using Orleans;
using Telegram.Bot.Types.InlineQueryResults;

namespace BotNet.GrainInterfaces {
	/// <summary>
	/// Key: search keywords, trimmed, converted to lowercase, not empty, concatenated with user Id
	/// </summary>
	public interface IInlineQueryGrain : IGrainWithStringKey {
		Task<ImmutableList<InlineQueryResult>> GetResultsAsync(string query, long userId, GrainCancellationToken grainCancellationToken);
	}
}
