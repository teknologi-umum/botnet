using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using BotNet.Services.Tenor;
using Orleans;

namespace BotNet.Grains {
	public class TenorGrain : Grain, ITenorGrain {
		private readonly TenorClient _tenorClient;

		public TenorGrain(
			TenorClient tenorClient
		) {
			_tenorClient = tenorClient;
		}

		public Task<ImmutableList<(string Id, string Url, string PreviewUrl)>> SearchGifsAsync() {
			return _tenorClient.SearchGifsAsync(this.GetPrimaryKeyString(), CancellationToken.None);
		}
	}
}
