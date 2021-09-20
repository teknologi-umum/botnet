using System;
using System.Collections.Immutable;
using System.Threading;
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

		public async Task<ImmutableList<SearchResultItem>> SearchAsync() {
			if (_searchResultItems is null) {
				string query = this.GetPrimaryKeyString();

				ImmutableList<SearchResultItem> resultItems = await _serviceProvider
					.GetRequiredService<DuckDuckGoClient>()
					.SearchAsync(query, CancellationToken.None);

				SafeSearchDictionary safeSearchDictionary = _serviceProvider.GetRequiredService<SafeSearchDictionary>();
				ImmutableList<SearchResultItem>.Builder safeResultItemsBuilder = ImmutableList.CreateBuilder<SearchResultItem>();
				foreach (SearchResultItem resultItem in resultItems) {
					if (await safeSearchDictionary.IsUrlAllowedAsync(resultItem.Url, CancellationToken.None)
						&& await safeSearchDictionary.IsContentAllowedAsync(resultItem.Title, CancellationToken.None)
						&& await safeSearchDictionary.IsUrlAllowedAsync(resultItem.IconUrl, CancellationToken.None)
						&& await safeSearchDictionary.IsContentAllowedAsync(resultItem.Snippet, CancellationToken.None)) {
						safeResultItemsBuilder.Add(resultItem);
					}
				}
				_searchResultItems = safeResultItemsBuilder.ToImmutable();
			}
			return _searchResultItems;
		}
	}
}
