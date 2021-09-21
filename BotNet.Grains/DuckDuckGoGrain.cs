using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BotNet.GrainInterfaces;
using BotNet.Services.DuckDuckGo;
using BotNet.Services.DuckDuckGo.Models;
using BotNet.Services.SafeSearch;
using Microsoft.Extensions.DependencyInjection;
using Orleans;

namespace BotNet.Grains {
	public class DuckDuckGoGrain : Grain, IDuckDuckGoGrain {
		private readonly IServiceProvider _serviceProvider;
		private ImmutableList<SearchResultItem>? _searchResultItems;

		public DuckDuckGoGrain(
			IServiceProvider serviceProvider
		) {
			_serviceProvider = serviceProvider;
		}

		public async Task<ImmutableList<SearchResultItem>> SearchAsync(GrainCancellationToken grainCancellationToken) {
			if (_searchResultItems is null) {
				string query = this.GetPrimaryKeyString();

				ImmutableList<SearchResultItem> resultItems = await _serviceProvider
					.GetRequiredService<DuckDuckGoClient>()
					.SearchAsync(query, grainCancellationToken.CancellationToken);

				SafeSearchDictionary safeSearchDictionary = _serviceProvider.GetRequiredService<SafeSearchDictionary>();
				ImmutableList<SearchResultItem>.Builder safeResultItemsBuilder = ImmutableList.CreateBuilder<SearchResultItem>();
				foreach (SearchResultItem resultItem in resultItems) {
					if (await safeSearchDictionary.IsUrlAllowedAsync(resultItem.Url, grainCancellationToken.CancellationToken)
						&& await safeSearchDictionary.IsContentAllowedAsync(resultItem.Url, grainCancellationToken.CancellationToken)
						&& await safeSearchDictionary.IsContentAllowedAsync(resultItem.Title, grainCancellationToken.CancellationToken)
						&& await safeSearchDictionary.IsUrlAllowedAsync(resultItem.IconUrl, grainCancellationToken.CancellationToken)
						&& await safeSearchDictionary.IsContentAllowedAsync(resultItem.Snippet, grainCancellationToken.CancellationToken)) {
						safeResultItemsBuilder.Add(resultItem);
					}
				}
				_searchResultItems = safeResultItemsBuilder.ToImmutable();
			}

			// Cache until there is no call in a 1 minute window
			DelayDeactivation(TimeSpan.FromMinutes(1));

			return _searchResultItems;
		}
	}
}
