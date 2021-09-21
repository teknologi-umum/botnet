using System.Collections.Immutable;
using System.Threading.Tasks;
using Orleans;

namespace BotNet.GrainInterfaces {
	/// <summary>
	/// Key: Last digit of user id
	/// </summary>
	public interface IDadJokeGrain : IGrainWithIntegerKey {
		Task<ImmutableList<(string Id, string Url)>> GetRandomJokesAsync(GrainCancellationToken grainCancellationToken);
	}
}
