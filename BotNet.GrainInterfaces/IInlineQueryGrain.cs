using System.Collections.Immutable;
using System.Threading.Tasks;
using Orleans;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace BotNet.GrainInterfaces {
	/// <summary>
	/// Key: search keywords, trimmed, converted to lowercase, not empty
	/// </summary>
	public interface IInlineQueryGrain : IGrainWithStringKey {
		Task<ImmutableList<InlineQueryResultBase>> GetResultsAsync(InlineQuery inlineQuery);
	}
}
