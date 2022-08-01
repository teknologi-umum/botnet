using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace BotNet.Services.TrustPositif {
	public class TrustPositifLookup {
		private readonly TrustPositifClient _client;

		public TrustPositifLookup(
			TrustPositifClient client
		) {
			_client = client;
		}

		public async Task<ImmutableDictionary<string, bool>> GetBlockStatusAsync(ImmutableHashSet<string> domains, CancellationToken cancellationToken) {
			if (domains.Count == 0) return ImmutableDictionary<string, bool>.Empty;

			ImmutableDictionary<string, bool>.Builder builder = ImmutableDictionary.CreateBuilder<string, bool>();

			foreach (string[] domainsChunk in domains.Chunk(100)) {
				ImmutableDictionary<string, bool> chunkResult = await _client.GetBlockStatusAsync(domainsChunk.ToImmutableHashSet(), cancellationToken);
				builder.AddRange(chunkResult);
			}

			return builder.ToImmutable();
		}
	}
}
