using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Orleans;
using Telegram.Bot.Types.InlineQueryResults;

namespace BotNet.GrainInterfaces {
	public interface IInlineQueryGrain : IGrainWithStringKey {
		Task<ImmutableList<InlineQueryResultBase>> GetResultsAsync();
	}
}
