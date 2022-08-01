using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BotNet.Services.TrustPositif.JsonModels;

namespace BotNet.Services.TrustPositif {
	public class TrustPositifClient {
		private const string URL = "https://trustpositif.kominfo.go.id/Rest_server/getrecordsname_home";
		private readonly HttpClient _httpClient;

		public TrustPositifClient(
			HttpClient httpClient
		) {
			_httpClient = httpClient;
		}

		public async Task<ImmutableDictionary<string, bool>> GetBlockStatusAsync(ImmutableHashSet<string> domains, CancellationToken cancellationToken) {
			if (domains.Count == 0) return ImmutableDictionary<string, bool>.Empty;
			if (domains.Count > 100) throw new ArgumentException("Can only query up to 100 domains at a time", nameof(domains));
			if (domains.Any(domain => domain.Contains("/") || domain.Contains(":"))) throw new ArgumentException("Some domain are invalid.", nameof(domains));

			// Send request
			using HttpRequestMessage request = new(HttpMethod.Post, URL);
			request.Content = new FormUrlEncodedContent(
				new Dictionary<string, string> {
					{ "name", string.Join('\n', domains) }
				}
			);
			using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

			// Ensure success			
			response.EnsureSuccessStatusCode();

			// Read response as json
			string json = await response.Content.ReadAsStringAsync(cancellationToken);
			return JsonSerializer.Deserialize<BlacklistLookupResult[]>(json)?.ToImmutableDictionary(
				keySelector: b => b.Domain,
				elementSelector: b => b.Status == "Ada"
			) ?? throw new HttpRequestException();
		}
	}
}
