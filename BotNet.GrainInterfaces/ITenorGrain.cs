using System.Collections.Immutable;
using System.Threading.Tasks;
using Orleans;

namespace BotNet.GrainInterfaces {
	/// <summary>
	/// Key: search keywords, trimmed, converted to lowercase, not empty
	/// </summary>
	public interface ITenorGrain : IGrainWithStringKey {
		Task<ImmutableList<(string Id, string Url, string PreviewUrl)>> SearchGifsAsync();
	}
}
