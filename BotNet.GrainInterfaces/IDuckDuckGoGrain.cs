using System.Collections.Immutable;
using System.Threading.Tasks;
using BotNet.Services.DuckDuckGo.Models;
using Orleans;

namespace BotNet.GrainInterfaces {
	/// <summary>
	/// Key: search query, trimmed, to lowercase
	/// </summary>
	public interface IDuckDuckGoGrain : IGrainWithStringKey {
		Task<ImmutableList<SearchResultItem>> SearchAsync();
	}
}
