using System;
using System.Collections.Immutable;
using BotNet.Services.PSE.JsonModels;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BotNet.Services.PSE {
	public class PSECrawler {
		private readonly PSEClient _client;
		private readonly ILogger<PSECrawler> _logger;

		public PSECrawler(
			PSEClient client,
			ILogger<PSECrawler> logger
		) {
			_client = client;
			_logger = logger;
		}

		public async Task<ImmutableList<DigitalService>> SearchAsync(string keyword, int take, CancellationToken cancellationToken) {
			try {
				(ImmutableList<DigitalService> digitalServices, int totalRows) = await _client.SearchAsync(
					keyword: keyword,
					length: take,
					start: 0,
					cancellationToken: cancellationToken
				);

				_logger.LogInformation("Found {Count} results for keyword '{Keyword}' out of {TotalRows} total", digitalServices.Count, keyword, totalRows);

				return digitalServices;
			} catch (Exception exc) {
				_logger.LogError(exc, "Error searching PSE for keyword '{Keyword}'", keyword);
				return ImmutableList<DigitalService>.Empty;
			}
		}
	}
}
